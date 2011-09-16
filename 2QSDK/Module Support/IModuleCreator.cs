using System;
using System.Collections.Generic;
using System.Text;

namespace Project2Q.SDK.ModuleSupport {

    /// <summary>
    /// A class that helps develop modules.
    /// </summary>
    public abstract class IModuleCreator {

        protected ModuleProxy mp;
        protected int moduleId;
        public string[] returns;

        /// <summary>
        /// Initializes the module with values.
        /// </summary>
        /// <param name="mp">The module proxy object.</param>
        /// <param name="mid">The module ID.</param>
        public virtual void Initialize(ModuleProxy mp, int mid) {
            this.mp = mp;
            this.moduleId = mid;
        }

        /// <summary>
        /// This is the method for modules to override if they want to add random stuff to initialization.
        /// </summary>
        public virtual void Initialize() {

        }

        /// <summary>
        /// This method is called when your module is hooked up to a server.
        /// Use this to hook up per-server events.
        /// </summary>
        /// <param name="sid">Server id of the server that has the current module active on it.</param>
        public virtual void Activated(int sid) {

        }

        /// <summary>
        /// This method is called when your module is activated on all servers.
        /// Use this to hook up all-server events.
        /// </summary>
        public virtual void ActivationComplete() {

        }

        /// <summary>
        /// This method should not be used to unregister events as all events and parses are automatically unregistered.
        /// </summary>
        /// <param name="sid">The server id that the module's been deactivated on.</param>
        public virtual void Deactivated(int sid) {

        }

    }

}
