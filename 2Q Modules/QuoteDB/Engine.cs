using System;
using System.Collections.Generic;
using System.Text;

using Project2Q.SDK;
using Project2Q.SDK.ModuleSupport;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ChannelSystem;
using Injections = Project2Q.SDK.Injections;

namespace QuoteDB {

    public class Engine : IModuleCreator, IDisposable {

        public override void Initialize() {

        }

        public override void ActivationComplete() {
            
        }

        #region IDisposable Members

        void IDisposable.Dispose() {
            
        }

        #endregion
    }

}
