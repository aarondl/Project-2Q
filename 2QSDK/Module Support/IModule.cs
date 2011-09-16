using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace Project2Q.SDK.ModuleSupport {

    public abstract class IModule {

        public static readonly int MaxModules = 32;
        public static readonly int MaxServers = 32;

        static IModule() {
            active = new int[MaxModules];
        }

        private static int[] active;

        /// <summary>
        /// Returns the list of integers that show which module is active on which server.
        /// </summary>
        public static int[] ActiveList {
            get { return active; }
        }

        /// <summary>
        /// Activate the module on a server.
        /// </summary>
        /// <param name="sid">The server to activate on.</param>
        public void Activate(int sid) {
            active[moduleId] |= ( 1 << sid );
            moduleProxy.Activate( sid );
        }

        /// <summary>
        /// Tells the module that it can safely register global events as all servers it's on have been activated.
        /// </summary>
        public void ActivationComplete() {
            moduleProxy.ActivationComplete();
        }

        /// <summary>
        /// Deactivate a module on a server.
        /// </summary>
        /// <param name="sid">The server the module is inactive on now.</param>
        public void Deactivate(int sid) {
            active[moduleId] &= ~( 1 << sid );
            moduleProxy.Deactivate( sid );
        }

        /// <summary>
        /// Determines if the module is active on a particular server.
        /// </summary>
        /// <param name="mid">Module ID</param>
        /// <param name="sid">Server ID</param>
        /// <returns>Is active?</returns>
        public static bool IsActive(int mid, int sid) {
            return ( active[mid] & ( 1 << sid ) ) > 0;
        }

        /// <summary>
        /// Determines if the module is active on a particular server.
        /// </summary>
        /// <param name="sid">The server id to check for activeness on.</param>
        /// <returns>Is Active?</returns>
        public bool IsActive(int sid) {
            return IsActive( moduleId, sid );
        }

        protected AppDomain moduleSpace;
        protected ModuleProxy moduleProxy;
        public abstract void LoadModule();
        public abstract void UnloadModule();
        protected int moduleId;
        protected Configuration.ModuleConfig modConfig;

        /// <summary>
        /// Determines if the module is loaded or not.
        /// </summary>
        public bool IsLoaded {
            get { return moduleSpace != null; }
        }

        /// <summary>
        /// Gets the module proxy for this module.
        /// </summary>
        public ModuleProxy ModuleProxy {
            get { return moduleProxy; }
        }

        /// <summary>
        /// Gets the ModuleConfig for this module.
        /// </summary>
        public Configuration.ModuleConfig ModuleConfig {
            get { return modConfig; }
        }

        /// <summary>
        /// Gets the module id for this module.
        /// </summary>
        public int ModuleId {
            get { return moduleId; }
        }

        /// <summary>
        /// Gets the appdomain for the module.
        /// </summary>
        public AppDomain ModuleSpace {
            get { return moduleSpace; }
        }

        /// <summary>
        /// Deliver full trust unto the Application Domain.
        /// </summary>
        /// <param name="ad">The application Domain to apply the security level to.</param>
        public static void SetSecurityPolicy(AppDomain ad) {

            PolicyLevel pLevel = PolicyLevel.CreateAppDomainLevel();
            PermissionSet ps = new PermissionSet( PermissionState.Unrestricted );
            UnionCodeGroup rootCodeGroup = new UnionCodeGroup( new AllMembershipCondition(),
                new PolicyStatement( ps, PolicyStatementAttribute.Nothing ) );
            
            pLevel.RootCodeGroup = rootCodeGroup;
            ad.SetAppDomainPolicy( pLevel );
        }

    }

}
