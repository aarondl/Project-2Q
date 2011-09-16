using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Security;

using Project2Q.SDK;
using Project2Q.SDK.ModuleSupport;

namespace Project2Q.Core {

    internal sealed class CompiledModule : IModule {

        /// <summary>
        /// Creates a module and readies it for loading.
        /// </summary>
        /// <param name="moduleConfiguration">The configuration of the module.</param>
        /// <param name="moduleId">The id number of the module relative the server.</param>
        /// <param name="bindTo">The server to bind the module events to.</param>
        public CompiledModule(Configuration.ModuleConfig moduleConfiguration, int moduleId) {
            this.modConfig = moduleConfiguration;
            this.moduleId = moduleId;
            this.moduleSpace = null;
        }

        /// <summary>
        /// Loads the module based on the configuration passed in the constructor.
        /// </summary>
        /// <returns>Success?</returns>
        public override void LoadModule() {

            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            ads.LoaderOptimization = LoaderOptimization.SingleDomain;
            ads.ShadowCopyFiles = "false";

            //Create the AppDomain.
            System.Security.Policy.Evidence secureEvidence = new System.Security.Policy.Evidence();
            secureEvidence.AddAssembly( Assembly.GetCallingAssembly() );
            moduleSpace = AppDomain.CreateDomain(
                "Project2QDomain." + moduleId,
                secureEvidence,
                ads );

            //FullTrust this guy.
            IModule.SetSecurityPolicy( moduleSpace );

            moduleProxy = new ModuleProxy(
                moduleId,
                "Project2QAssembly." + moduleId,
                modConfig.FileNames,
                modConfig.FullName,
                new Project2Q.SDK.ModuleSupport.ModuleProxy.VariableParamRetrievalDelegate( Server.RetrieveVariable ));

            try {
                moduleProxy.LoadModule();
            }
            catch {
                AppDomain.Unload( moduleSpace );
                moduleSpace = null;
                moduleProxy = null;
                throw;
            }

        }

        /// <summary>
        /// Unloads the module.
        /// </summary>
        /// <returns>Success?</returns>
        public override void UnloadModule() {

            if ( moduleSpace == null ) return;

            moduleProxy.UnregisterAllEvents();

            IDisposable id = moduleProxy.ModuleInstance as IDisposable;
            if ( id != null )
                id.Dispose();

            moduleProxy = null; //We will have to recreate this object to reload the DLL/Script.

            AppDomain.Unload( moduleSpace ); //Unload the application domain.
            moduleSpace = null;

        }
    }

}
