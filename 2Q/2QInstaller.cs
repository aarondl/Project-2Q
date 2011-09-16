using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace Project2Q.Core {

    /// <summary>
    /// The installation class for this project. Installs a Windows Service
    /// using the installUtil or the generated 2QInstaller project.
    /// </summary>
    [RunInstaller(true)]
    public class Project2QInstaller : Installer {

        private ServiceInstaller Project2QServiceInstaller;
        private ServiceProcessInstaller Project2QServiceProcessInstaller;

        public Project2QInstaller() {

            Project2QServiceInstaller = new ServiceInstaller();
            Project2QServiceProcessInstaller = new ServiceProcessInstaller();

            Project2QServiceProcessInstaller.Account = ServiceAccount.LocalSystem;

            Project2QServiceInstaller.StartType = ServiceStartMode.Manual;
            Project2QServiceInstaller.ServiceName = "2Q";
            Project2QServiceInstaller.DisplayName = "Project 2Q";
            Project2QServiceInstaller.Description = "Modularized IRC Bot";

            Installers.Add( Project2QServiceInstaller );
            Installers.Add( Project2QServiceProcessInstaller );

        }

    }

}
