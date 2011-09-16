using System;
using System.Collections.Generic;
using System.Text;

using Project2Q.SDK;
using Project2Q.SDK.UserSystem;
using Project2Q.SDK.ModuleSupport;
using Project2Q.SDK.Injections;

namespace ModuleControl {

    /// <summary>
    /// A class built on controlling modules remotely.
    /// </summary>
    public class ModControl : IModuleCreator {

        /// <summary>
        /// Initialize the module.
        /// </summary>
        public override void Initialize() {
            mp.RegisterParse( "umod", new CrossAppDomainDelegate( UnloadModule ), 
                IRCEvents.ParseTypes.ChannelMessage | IRCEvents.ParseTypes.PrivateMessage );
            mp.RegisterParse( "lmod", new CrossAppDomainDelegate( UnloadModule ), 
                IRCEvents.ParseTypes.ChannelMessage | IRCEvents.ParseTypes.PrivateMessage );
        }

        public ChannelMessageEvent channelMessageData;
        public UserMessageEvent userMessageData;

        [PrivelegeRequired( Priveleges.SuperUser )]
        public void LoadModule() {
            
        }

        [PrivelegeRequired( Priveleges.SuperUser )]
        public void UnloadModule() {
        }

    }

}
