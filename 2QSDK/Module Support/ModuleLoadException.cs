using System;
using System.Collections.Generic;
using System.Text;

namespace Project2Q.SDK.ModuleSupport {
    
    /// <summary>
    /// Thrown when a module fails to load.
    /// </summary>
    public class ModuleLoadException : Exception {

        private string[] compilerErrors;

        /// <summary>
        /// If the ModuleLoadException resulted from compiler errors
        /// this field will contain the errors.
        /// </summary>
        public string[] CompilerErrors {
            get { return compilerErrors; }
        }

        /// <summary>
        /// Constructs a base Exception with a message.
        /// </summary>
        /// <param name="message">The message to relay to the code trying to load the module.</param>
        public ModuleLoadException(string message)
            : base( message ) {
            compilerErrors = null;
        }

        /// <summary>
        /// Constructs a ModuleLoadException with compiler errors.
        /// </summary>
        /// <param name="message">The message to relay to the code trying to load the module.</param>
        /// <param name="compilerErrors">The compiler errors that occurred.</param>
        public ModuleLoadException(string message, string[] compilerErrors)
            : base( message ) {
            this.compilerErrors = compilerErrors;
        }

    }

}
