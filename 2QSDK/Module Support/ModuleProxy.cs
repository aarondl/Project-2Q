using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

using Project2Q.SDK.UserSystem;

namespace Project2Q.SDK.ModuleSupport {

    /// <summary>
    /// An enumeration that will allow modules to request from the server
    /// different variables.
    /// </summary>
    public enum Request {
        /// <summary>
        /// Retrieves the active configuration object. (Return type: Configuration) [Note: Serverid is ignored for this param.]
        /// </summary>
        Configuration,
        /// <summary>
        /// Retrieves the active Server configuration object. (Return type: Configuration.ServerConfig)
        /// </summary>
        ServerConfiguration,
        /// <summary>
        /// Retrieves the active ChannelCollection. (Return type: ChannelCollection)
        /// </summary>
        ChannelCollection,
        /// <summary>
        /// Retrieves the active UserCollection. (Return type: UserCollection)
        /// </summary>
        UserCollection,
        /// <summary>
        /// TODO: Fix this so it returns something useful for the server. Retrieves the active ModuleList. (Return type: IModule[])
        /// </summary>
        ModuleList,
        /// <summary>
        /// The IRCEvents instance for the server.
        /// </summary>
        IRCEvents,
        /// <summary>
        /// A function pointer to SendData
        /// </summary>
        SendData,
        /// <summary>
        /// The current remote (read: not local) IP of the server. Can be null if unknown.
        /// </summary>
        CurrentIP,
    }

    /// <summary>
    /// This class is the true proxy object that will go between
    /// the modules/scripts and invoke their events that have been
    /// written in.
    /// </summary>
    public sealed class ModuleProxy : MarshalByRefObject {

        #region Statics

        private static EventInfo[] eventList;

        /// <summary>
        /// Creates the event list to search for module functions with.
        /// </summary>
        static ModuleProxy() {
            eventList = typeof( ModuleEvents ).GetEvents( BindingFlags.Instance | BindingFlags.Public );

            Algorithms.QuickSort.EventInfoQuickSort( ref eventList );
        }

        #endregion

        #region Data and Properties

        private Assembly assembly;
        private string assemblyName;
        private Object instanceOfModule;
        private string[] filenames;
        private string classname;
        private int moduleId;
        private ModuleEvents moduleEvents;
        private VariableParamRetrievalDelegate serverParamRetrievalFunction;

        /// <summary>
        /// Get's the assembly for the module.
        /// </summary>
        public Assembly ModuleAssembly {
            get { return assembly; }
        }

        /// <summary>
        /// Get's the instance of this module.
        /// </summary>
        public object ModuleInstance {
            get { return instanceOfModule; }
        }

        /// <summary>
        /// Get's the filenames for the current module.
        /// </summary>
        public string[] Filenames {
            get { return filenames; }
        }

        /// <summary>
        /// Get's the module ID number.
        /// </summary>
        public int ModuleID {
            get { return moduleId; }
        }

        #endregion

        #region Activation

        /// <summary>
        /// Tells a module it's been activated on the server matching sid.
        /// </summary>
        /// <param name="sid">The server that's had the module activated on it.</param>
        public void Activate(int sid) {
            IModuleCreator imc = instanceOfModule as IModuleCreator;

            if ( imc != null )
                imc.Activated( sid );
        }

        /// <summary>
        /// Tells a module all activation for itself is done, and it can register global events safely.
        /// </summary>
        public void ActivationComplete() {
            IModuleCreator imc = instanceOfModule as IModuleCreator;

            if ( imc != null )
                imc.ActivationComplete();
        }

        /// <summary>
        /// Tells a module it's been deactivated on the server matching sid.
        /// </summary>
        /// <param name="sid">The server the module is deactivated on.</param>
        public void Deactivate(int sid) {
            IModuleCreator imc = instanceOfModule as IModuleCreator;

            if ( imc != null )
                imc.Deactivated( sid );

            UnregisterAllEvents( sid );
            UnregisterAllParses( sid );
        }

        #endregion

        #region Event Registration

        /// <summary>
        /// Registers an event.
        /// </summary>
        /// <param name="eventName">The event name to register.</param>
        /// <param name="serverid">The server register the event to.</param>
        /// <param name="function">The function to register to the event.</param>
        /// <returns>Registration success?</returns>
        public bool RegisterEvent(string eventName, int serverid, CrossAppDomainDelegate function) {

            int loc = Algorithms.BinarySearch.EventInfoBinarySearch( eventList, eventName );
            if ( loc < 0 )
                return false;

            //Get the method we need to create a delegate to.
            MethodInfo mi = typeof( ModuleEvents ).GetMethod( "On" + eventName );
            //Get the event we need to attach the delegate to.
            EventInfo ei = typeof( IRCEvents ).GetEvent( eventName );
            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            //When we add an event from IRCEvents --- CALLS --> OnEvent in ModuleEvents.
            //We can't let it add twice. ModuleEvents keeps it's own list of methods to call in the module it's on.
            //So if the method we would add to IRCEvents' handler is found in the IRCEvents.EventName delegate
            //we should not add it PROVIDING that the moduleEvents target on that handler is the same object as the one
            //we hold in our current ModuleProxy object.
            FieldInfo fi = typeof( IRCEvents ).GetField( eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField );
            object o = fi.GetValue( serversIrcEvents );
            Delegate d = (Delegate)o;
            bool addToIRCEvents = true;
            if ( d != null ) {
                Delegate[] invocList = d.GetInvocationList();
                foreach ( Delegate invoc in invocList ) {
                    if ( invoc.Method.Equals( mi ) && this.moduleEvents.GetHashCode() == invoc.Target.GetHashCode() )
                        addToIRCEvents = false;
                }
            }

            if ( addToIRCEvents )
            // Servers IRCEvents Object --- CALLS --> OnPing in ModuleEvents
            ei.AddEventHandler( serversIrcEvents, Delegate.CreateDelegate( ei.EventHandlerType, moduleEvents, mi ) );

            // ModuleLoader's ModuleEvents Object ---- CALLS --> 'function' inside the Module.
            (typeof( ModuleEvents ).GetEvent( eventName, BindingFlags.Public | BindingFlags.Instance ))
            .AddEventHandler( this.moduleEvents, function );

            return true;
        }

        /// <summary>
        /// Registers an event to all servers.
        /// </summary>
        /// <param name="eventName">The event name to register.</param>
        /// <param name="function">The function to register to the event.</param>
        /// <returns>Registration success?</returns>
        public bool RegisterEvent(string eventName, CrossAppDomainDelegate function) {

            int loc = Algorithms.BinarySearch.EventInfoBinarySearch( eventList, eventName );
            if ( loc < 0 )
                return false;

            //Get the method we need to create a delegate to.
            MethodInfo mi = typeof( ModuleEvents ).GetMethod( "On" + eventName );
            //Get the event we need to attach the delegate to.
            EventInfo ei = typeof( IRCEvents ).GetEvent( eventName );

            for ( int i = 0; i < IModule.MaxServers; i++ ) {

                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, i );

                if ( serversIrcEvents == null ) break;

                //When we add an event from IRCEvents --- CALLS --> OnEvent in ModuleEvents.
                //We can't let it add twice. ModuleEvents keeps it's own list of methods to call in the module it's on.
                //So if the method we would add to IRCEvents' handler is found in the IRCEvents.EventName delegate
                //we should not add it PROVIDING that the moduleEvents target on that handler is the same object as the one
                //we hold in our current ModuleProxy object.
                FieldInfo fi = typeof( IRCEvents ).GetField( eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField );
                object o = fi.GetValue( serversIrcEvents );
                Delegate d = (Delegate)o;
                bool addToIRCEvents = true;
                if ( d != null ) {
                    Delegate[] invocList = d.GetInvocationList();
                    foreach ( Delegate invoc in invocList ) {
                        if ( invoc.Method.Equals( mi ) && this.moduleEvents.GetHashCode() == invoc.Target.GetHashCode() )
                            addToIRCEvents = false;
                    }
                }

                if ( addToIRCEvents )
                    // Servers IRCEvents Object --- CALLS --> OnPing in ModuleEvents
                    ei.AddEventHandler( serversIrcEvents, Delegate.CreateDelegate( ei.EventHandlerType, moduleEvents, mi ) );

                // ModuleLoader's ModuleEvents Object ---- CALLS --> 'function' inside the Module.
                ( typeof( ModuleEvents ).GetEvent( eventName, BindingFlags.Public | BindingFlags.Instance ) )
                .AddEventHandler( this.moduleEvents, function );

            }

            return true;
        }

        /// <summary>
        /// Unregisters an event.
        /// </summary>
        /// <param name="eventName">The event name to unregister.</param>
        /// <param name="serverid">The server to unregister the event from.</param>
        /// <param name="function">The function to register to the event.</param>
        /// <returns>Unregistration success?</returns>
        public bool UnregisterEvent(string eventName, int serverid, CrossAppDomainDelegate function) {

            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            int loc = Algorithms.BinarySearch.EventInfoBinarySearch( eventList, eventName );
            if ( loc < 0 )
                return false;

            //Get the method we need to create a delegate to.
            MethodInfo mi = typeof( ModuleEvents ).GetMethod( "On" + eventName );
            //Get the event we need to attach the delegate to.
            EventInfo ei = typeof( IRCEvents ).GetEvent( eventName );

            // Servers IRCEvents Object --- CALLS --> OnPing in ModuleEvents
            ei.AddEventHandler( serversIrcEvents, Delegate.CreateDelegate( ei.EventHandlerType, moduleEvents, mi ) );

            // ModuleLoader's ModuleEvents Object ---- CALLS --> 'function' inside the Module.
            ( typeof( ModuleEvents ).GetEvent( eventName, BindingFlags.Public | BindingFlags.Instance ) )
            .RemoveEventHandler( this.moduleEvents, function );

            return true;
        }

        /// <summary>
        /// Unregisters an event on all servers.
        /// </summary>
        /// <param name="eventName">The event name to unregister.</param>
        /// <param name="function">The function to register to the event.</param>
        /// <returns>Unregistration success?</returns>
        public bool UnregisterEvent(string eventName, CrossAppDomainDelegate function) {

            for ( int i = 0; i < IModule.MaxServers; i++ ) {

                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, i );
                if ( serversIrcEvents == null ) break;

                int loc = Algorithms.BinarySearch.EventInfoBinarySearch( eventList, eventName );
                if ( loc < 0 )
                    return false;

                //Get the method we need to create a delegate to.
                MethodInfo mi = typeof( ModuleEvents ).GetMethod( "On" + eventName );
                //Get the event we need to attach the delegate to.
                EventInfo ei = typeof( IRCEvents ).GetEvent( eventName );

                // Servers IRCEvents Object --- CALLS --> OnPing in ModuleEvents
                ei.AddEventHandler( serversIrcEvents, Delegate.CreateDelegate( ei.EventHandlerType, moduleEvents, mi ) );

                // ModuleLoader's ModuleEvents Object ---- CALLS --> 'function' inside the Module.
                ( typeof( ModuleEvents ).GetEvent( eventName, BindingFlags.Public | BindingFlags.Instance ) )
                .RemoveEventHandler( this.moduleEvents, function );

            }

            return true;
        }

        /// <summary>
        /// Unregisters all events from a server.
        /// </summary>
        /// <param name="serverid">The server to remove the events from.</param>
        public void UnregisterAllEvents(int serverid) {

            Type irce = typeof(IRCEvents);
            Type mode = typeof(ModuleEvents);
            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            EventInfo[] ircEvents = irce.GetEvents( BindingFlags.Public | BindingFlags.Instance );
            EventInfo[] modEvents = mode.GetEvents( BindingFlags.Public | BindingFlags.Instance );

            //Disconnect IRCEvents from OnEvent handlers in ModuleEvents
            foreach ( EventInfo i in ircEvents ) {
                MethodInfo mi = mode.GetMethod( "On" + i.Name );
                if ( mi != null )
                    i.RemoveEventHandler( serversIrcEvents, 
                        Delegate.CreateDelegate( i.EventHandlerType, moduleEvents, mi ));
            }

            //Disconnect ModuleEvents from the Module's handlers.
            foreach ( EventInfo i in modEvents ) {
                MethodInfo mi = instanceOfModule.GetType().GetMethod( "On" + i.Name );
                if ( mi != null )
                    i.RemoveEventHandler( moduleEvents,
                        Delegate.CreateDelegate( i.EventHandlerType, instanceOfModule, mi) );
            }

        }


        /// <summary>
        /// Unregisters all events from all servers.
        /// </summary>
        public void UnregisterAllEvents() {

            Type irce = typeof( IRCEvents );
            Type mode = typeof( ModuleEvents );

            EventInfo[] ircEvents = irce.GetEvents( BindingFlags.Public | BindingFlags.Instance );
            EventInfo[] modEvents = mode.GetEvents( BindingFlags.Public | BindingFlags.Instance );

            for ( int j = 0; j < IModule.MaxServers; j++ ) {

                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, j );
                if ( serversIrcEvents == null ) break;

                //Disconnect IRCEvents from OnEvent handlers in ModuleEvents
                foreach ( EventInfo i in ircEvents ) {
                    MethodInfo mi = mode.GetMethod( "On" + i.Name );
                    if ( mi != null )
                        i.RemoveEventHandler( serversIrcEvents,
                            Delegate.CreateDelegate( i.EventHandlerType, moduleEvents, mi ) );
                }

                //Disconnect ModuleEvents from the Module's handlers.
                foreach ( EventInfo i in modEvents ) {
                    MethodInfo mi = instanceOfModule.GetType().GetMethod( "On" + i.Name );
                    if ( mi != null )
                        i.RemoveEventHandler( moduleEvents,
                            Delegate.CreateDelegate( i.EventHandlerType, instanceOfModule, mi ) );
                }
            }

        }

        #endregion

        #region Parser Registration

        /// <summary>
        /// Enables a module to register a parse that will be activated on
        /// the events specified by the parsetype.
        /// </summary>
        /// <param name="parse">The parse to register.</param>
        /// <param name="serverid">The server to register the parse to.</param>
        /// <param name="d">The function to call.</param>
        /// <param name="parseType">The events to add the parse to.</param>
        public void RegisterParse(string parse, int serverid, CrossAppDomainDelegate d, IRCEvents.ParseTypes parseType) {
            IRCEvents.Parse p = new IRCEvents.Parse( parse, d, parseType ,
                instanceOfModule.GetType().GetField("parseReturns", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.SetField ),
                instanceOfModule
                );

            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            if ( serversIrcEvents.Parses.Count == 0 )
                serversIrcEvents.Parses.Add( p );
            else {
                for ( int i = 0; i < serversIrcEvents.Parses.Count; i++ )
                    if ( serversIrcEvents.Parses[i].ParseString.CompareTo( parse ) >= 0 ) {
                        serversIrcEvents.Parses.Insert( i, p ); //I hope to god this shoves to the right and not the left.
                        return;
                    }
                serversIrcEvents.Parses.Insert( serversIrcEvents.Parses.Count - 1, p );
            }
        }

        /// <summary>
        /// Enables a module to register a parse that will be activated on
        /// the events specified by the parsetype on all servers.
        /// </summary>
        /// <param name="parse">The parse to register.</param>
        /// <param name="d">The function to call.</param>
        /// <param name="parseType">The events to add the parse to.</param>
        public void RegisterParse(string parse, CrossAppDomainDelegate d, IRCEvents.ParseTypes parseType) {
            IRCEvents.Parse p = new IRCEvents.Parse( parse, d, parseType,
                instanceOfModule.GetType().GetField( "parseReturns", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.SetField ),
                instanceOfModule
                );

            for ( int k = 0; k < IModule.MaxServers; k++ ) {

                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, k );
                if ( serversIrcEvents == null ) break;

                if ( serversIrcEvents.Parses.Count == 0 )
                    serversIrcEvents.Parses.Add( p );
                else {
                    for ( int i = 0; i < serversIrcEvents.Parses.Count; i++ )
                        if ( serversIrcEvents.Parses[i].ParseString.CompareTo( parse ) >= 0 ) {
                            serversIrcEvents.Parses.Insert( i, p ); //I hope to god this shoves to the right and not the left.
                            return;
                        }
                    serversIrcEvents.Parses.Insert( serversIrcEvents.Parses.Count - 1, p );
                }

            }
        }

        /// <summary>
        /// Enables a module to register a parse that will be activated on
        /// the events specified by the parsetype.
        /// </summary>
        /// <param name="parse">The parse to register.</param>
        /// <param name="serverid">The server to register the parse to.</param>
        /// <param name="d">The function to call.</param>
        /// <param name="parseType">The events to add the parse to.</param>
        public void RegisterWildcardParse(string parse, int serverid, CrossAppDomainDelegate d, IRCEvents.ParseTypes parseType) {
            IRCEvents.Parse p = new IRCEvents.Parse( parse, d, parseType,
                instanceOfModule.GetType().GetField("parseReturns", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.SetField ),
                instanceOfModule
                );

            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            serversIrcEvents.WildcardParses.Add( p );
        }

        /// <summary>
        /// Enables a module to register a parse that will be activated on
        /// the events specified by the parsetype on all servers.
        /// </summary>
        /// <param name="parse">The parse to register.</param>
        /// <param name="d">The function to call.</param>
        /// <param name="parseType">The events to add the parse to.</param>
        public void RegisterWildcardParse(string parse, CrossAppDomainDelegate d, IRCEvents.ParseTypes parseType) {
            IRCEvents.Parse p = new IRCEvents.Parse( parse, d, parseType,
                instanceOfModule.GetType().GetField( "parseReturns", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.SetField ),
                instanceOfModule
                );

            for ( int i = 0; i < IModule.MaxServers; i++ ) {
                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, i );
                if ( serversIrcEvents == null ) break;

                serversIrcEvents.WildcardParses.Add( p );
            }
        }

        /// <summary>
        /// Unregisters a modules parse.
        /// </summary>
        /// <param name="parse">The parse to remove.</param>
        /// <param name="serverid">The server id to remove the parse from.</param>
        public void UnregisterParse(string parse, int serverid) {

            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            int r = Algorithms.BinarySearch.ParseBinarySearch( serversIrcEvents.Parses, parse );
            if ( r > 0 ) {
                serversIrcEvents.Parses.RemoveAt( r );
                return;
            }

            for ( int i = 0; i < serversIrcEvents.WildcardParses.Count; i++ ) {
                if ( serversIrcEvents.WildcardParses[i].ParseString.Equals( parse ) ) {
                    serversIrcEvents.WildcardParses.RemoveAt( i );
                    return;
                }
            }
        }

        /// <summary>
        /// Unregisters a modules parse.
        /// </summary>
        /// <param name="parse">The parse to remove.</param>
        public void UnregisterParse(string parse) {

            for ( int k = 0; k < IModule.MaxServers; k++ ) {

                IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, k );

                if ( serversIrcEvents == null ) break;

                int r = Algorithms.BinarySearch.ParseBinarySearch( serversIrcEvents.Parses, parse );
                if ( r > 0 ) {
                    serversIrcEvents.Parses.RemoveAt( r );
                    return;
                }

                for ( int i = 0; i < serversIrcEvents.WildcardParses.Count; i++ ) {
                    if ( serversIrcEvents.WildcardParses[i].ParseString.Equals( parse ) ) {
                        serversIrcEvents.WildcardParses.RemoveAt( i );
                        return;
                    }
                }

            }
        }

        /// <summary>
        /// Unregisters all of a modules' parses on a server.
        /// </summary>
        /// <param name="serverid">The server to remove the parses from.</param>
        public void UnregisterAllParses(int serverid) {

            IRCEvents serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, serverid );

            serversIrcEvents.Parses.RemoveAll(new Predicate<IRCEvents.Parse>(UnregisterPredicate));
            serversIrcEvents.WildcardParses.RemoveAll(new Predicate<IRCEvents.Parse>(UnregisterPredicate));

        }

        /// <summary>
        /// Unregisters all of a modules' parses.
        /// </summary>
        public void UnregisterAllParses() {

            IRCEvents serversIrcEvents = null;

            for ( int i = 0; i < IModule.MaxServers; i++ ) { //TODO: Possibly allow for non hack way of stopping this?
                serversIrcEvents = (IRCEvents)RequestVariable( Request.IRCEvents, i );
                if ( serversIrcEvents == null ) break;
                serversIrcEvents.Parses.RemoveAll( new Predicate<IRCEvents.Parse>( UnregisterPredicate ) );
                serversIrcEvents.WildcardParses.RemoveAll( new Predicate<IRCEvents.Parse>( UnregisterPredicate ) );
            }

        }

        /// <summary>
        /// Aids in removal of module parses.
        /// </summary>
        /// <param name="p">The parse to check for removal.</param>
        private bool UnregisterPredicate(IRCEvents.Parse p) {
            return p.Function.Method.Module.Equals( instanceOfModule.GetType().Module );
        }

        #endregion

        #region Module Variable Retrieval Support

        public delegate bool SendDataDelegate(string text);
        public delegate object VariableParamRetrievalDelegate(Request variable, int sid);

        /// <summary>
        /// Requests a variable from the server object the module is on.
        /// </summary>
        /// <param name="var">The variable requested.</param>
        /// <param name="sid">The server the requested variable should be from.</param>
        /// <returns>The variable requested -- or null if unavailible.</returns>
        public object RequestVariable(Request var, int sid) {
            return this.serverParamRetrievalFunction( var, sid );
        }

        #endregion

        #region Cross Module Calls

        /// <summary>
        /// Provides an interface to call functions in other modules.
        /// </summary>
        /// <param name="Modulename">The module name according to the bot.</param>
        /// <param name="serverid">The server id of a server the module is active on.</param>
        /// <param name="FullFunctionname">The full function name.</param>
        /// <param name="preserveUserPriveleges">The User object to compare priveleges to.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return object or null.</returns>
        public object CrossModuleCall(string Modulename, int serverid, string FullFunctionname, UserSystem.User preserveUserPriveleges,
            params object[] parameters) {

            IModule[] imods = (IModule[])RequestVariable( Request.ModuleList, serverid);

            MethodInfo mi = null;
            object o = null;

            for ( int i = 0; i < imods.Length; i++ )
                if ( imods[i].IsLoaded )
                    if ( imods[i].ModuleConfig.FullName.Equals( Modulename ) ) {
                        mi = imods[i].ModuleProxy.ModuleInstance.GetType().GetMethod( FullFunctionname,
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod |
                            BindingFlags.Instance );

                        if ( mi != null ) {
                            o = imods[i].ModuleProxy.ModuleInstance;
                            break;
                        }
                    }

            if ( mi == null ) return null;

            PrivelegeContainer p = 
                preserveUserPriveleges.UserAttributes != null ? preserveUserPriveleges.UserAttributes.Privegeles :
                null;

            bool continueWithCall = true;

            if ( p == null || !p.HasPrivelege( Priveleges.SuperUser ) ) {

                //Check method permissions, can we call it?
                object[] privreq = mi.GetCustomAttributes( false );

                for ( int j = 0; j < privreq.Length; j++ ) {
                    if ( continueWithCall && privreq[j].GetType().Equals( typeof( PrivelegeRequiredAttribute ) ) ) {
                        PrivelegeRequiredAttribute pra = (PrivelegeRequiredAttribute)privreq[j];
                        continueWithCall = p != null ? p.HasPrivelege( pra.Required ) : ( (ulong)pra.Required == 0 );
                    }
                    if ( continueWithCall && privreq[j].GetType().Equals( typeof( UserLevelRequiredAttribute ) ) ) {
                        UserLevelRequiredAttribute ura = (UserLevelRequiredAttribute)privreq[j];
                        continueWithCall = p != null ? p.NumericalLevel >= ura.Required : ( ura.Required == 0 );
                    }

                    if ( !continueWithCall )
                        break;
                }

            }

            if ( !continueWithCall )
                return null;

            o = mi.Invoke( o, parameters );
            return o;

        }

        /// <summary>
        /// Provides an interface to call functions in other modules.
        /// </summary>
        /// <param name="Modulename">The module name according to the bot.</param>
        /// <param name="serverid">The id of a server the module is active on.</param>
        /// <param name="FullFunctionname">The full function name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return object or null.</returns>
        public object CrossModuleCall(string Modulename, int serverid, string FullFunctionname, params object[] parameters) {

            IModule[] imods = (IModule[])RequestVariable( Request.ModuleList, serverid );

            MethodInfo mi = null;
            object o = null;

            for ( int i = 0; i < imods.Length; i++ )
                if ( imods[i] != null && imods[i].IsLoaded )

                    if ( imods[i].ModuleConfig.FullName.Equals( Modulename ) ) {
                        imods[i].ModuleProxy.ModuleInstance.GetType().GetMethod( FullFunctionname ,
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod |
                            BindingFlags.Instance );

                        if ( mi != null ) {
                            o = imods[i].ModuleProxy.ModuleInstance;
                            break;
                        }
                    }

            if ( mi == null ) return null;

            o = mi.Invoke( o, parameters );
            return o;

        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates the class.
        /// </summary>
        /// <param name="moduleId">The id of the module.</param>
        /// <param name="assemblyName">The assembly name to generate.</param>
        /// <param name="filenames">The filename of the module.</param>
        /// <param name="classname">The class name of the module.</param>
        public ModuleProxy(int moduleId, string assemblyName, string[] filenames, string classname,
            VariableParamRetrievalDelegate vprd) {
            this.moduleId = moduleId;
            this.filenames = filenames;
            this.classname = classname;
            this.assemblyName = assemblyName;
            this.serverParamRetrievalFunction = vprd;
            this.moduleEvents = new ModuleEvents( this );
        }

        #endregion

        #region Loaders

        /// <summary>
        /// Loads a .NET DLL.
        /// </summary>
        /// <exception cref="ModuleLoadException">When the module fails to load this is thrown.</exception>
        public void LoadModule() {
            Type moduleType = null;

            if ( assembly == null )
                try {
                    if ( !System.IO.File.Exists( filenames[0] ) && Configuration.ModuleConfig.ModulePath != null )
                        filenames[0] = System.IO.Path.Combine(Configuration.ModuleConfig.ModulePath, filenames[0]);
                    filenames[0] = System.IO.Path.GetFullPath( filenames[0] );
                    assembly = Assembly.LoadFile( filenames[0] );
                }
                catch ( Exception ) {
                    throw new ModuleLoadException( "Could not load assembly: " + filenames[0] );
                }

            try {
                moduleType = assembly.GetType( classname, true, false );
            }
            catch ( Exception ) {
                throw new ModuleLoadException( "Type not found: " + classname );
            }

            bool createAnInstance = true;
            bool useParamList = true;

            ConstructorInfo classConstructor =
                moduleType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof( ModuleProxy ), typeof( int ) }, //Constructor arglist.
                null );

            if ( classConstructor == null ) {

                useParamList = false;

                classConstructor = moduleType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[0], null );

                if ( classConstructor == null )
                    createAnInstance = false;
            }

            useParamList = false; //Shortchange our system. See above.

            if ( createAnInstance && useParamList )
                instanceOfModule = classConstructor.Invoke( new object[] { this, (object)moduleId } );
            else if ( createAnInstance )
                instanceOfModule = classConstructor.Invoke( null );
            else
                instanceOfModule = null;

            IModuleCreator imc = instanceOfModule as IModuleCreator;
            if ( imc != null ) {
                imc.Initialize( this, moduleId );
                imc.Initialize();
            }

        }

        /// <summary>
        /// The types of script files supported.
        /// </summary>
        public enum CodeType {
            CSharp,
            VisualBasic,
        }

        /// <summary>
        /// Loads a C#/VB Script.
        /// </summary>
        /// <param name="includes">A array of string of referenced assembly file names.</param>
        /// <param name="ct">The CodeType of the script.</param>
        /// <exception cref="ModuleLoadException">When the module fails to load this is thrown.</exception>
        public void LoadScript(string[] includes, CodeType ct) {
            CodeDomProvider languageProvider = null;
            switch ( ct ) {
                case CodeType.CSharp:
                    languageProvider = new CSharpCodeProvider();
                    break;
                case CodeType.VisualBasic:
                    languageProvider = new VBCodeProvider();
                    break;
            }

            if ( languageProvider == null )
                throw new ModuleLoadException( "Compiler class for CodeType: " + ( (int)ct ).ToString() + " not found." );

            for ( int i = 0; i < filenames.Length; i++ ) {
                if ( !System.IO.File.Exists( filenames[i] ) && Configuration.ModuleConfig.ModulePath != null )
                    filenames[i] =
                        System.IO.Path.GetFullPath( System.IO.Path.Combine( Configuration.ModuleConfig.ModulePath, filenames[i] ) );
            }

            CompilerParameters cp = new CompilerParameters();
            cp.GenerateInMemory = true;
            cp.GenerateExecutable = false;
            cp.ReferencedAssemblies.AddRange( includes );
            cp.OutputAssembly = assemblyName;
            cp.IncludeDebugInformation = false;

            CompilerResults cr = languageProvider.CompileAssemblyFromFile(
                cp, filenames );

            if ( cr.Errors.Count > 0 ) {
                string[] errors = new string[cr.Errors.Count];
                for ( int i = 0; i < cr.Errors.Count; i++ )
                    errors[i] = cr.Errors[i].ToString();
                throw new ModuleLoadException( "Failed to compile", errors );
            }

            Type moduleType = null;
            assembly = cr.CompiledAssembly;

            try {
                moduleType = assembly.GetType( classname, true, false );
            }
            catch ( Exception ) {
                throw new ModuleLoadException( "Type not found: " + classname );
            }

            bool createAnInstance = true;
            bool useParamList = true;

            ConstructorInfo classConstructor =
                moduleType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof( ModuleProxy ), typeof( int ) }, //Constructor arglist.
                null );

            if ( classConstructor == null ) {

                useParamList = false;

                classConstructor = moduleType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[0], null );

                if ( classConstructor == null )
                    createAnInstance = false;
            }

            if ( createAnInstance && useParamList )
                instanceOfModule = classConstructor.Invoke( new object[] { this, (object)moduleId } );
            else if ( createAnInstance )
                instanceOfModule = classConstructor.Invoke( null );
            else
                instanceOfModule = null;

            useParamList = false; //Shortchange our system. See above.

            IModuleCreator imc = instanceOfModule as IModuleCreator;
            if ( imc != null ) {
                imc.Initialize( this, moduleId );
                imc.Initialize();
            }

        }

        #endregion

    }

}
