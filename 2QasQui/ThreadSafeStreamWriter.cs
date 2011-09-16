using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace _QasQui {
    public class ThreadSafeStreamWriter : StreamWriter {

        private Stream s;
        public delegate void WrittenToDelegate(string x);
        public event WrittenToDelegate GotWrittenTo;

        public ThreadSafeStreamWriter(Stream s) : base(s) {
            this.s = s;
        }

        public override void Write(string value) {
            Monitor.Enter( this );
            base.Write( value + "\0" );
            Monitor.Exit( this );
            GotWrittenTo( null );
        }

        public override void WriteLine(string value) {
            Monitor.Enter( this );
            base.WriteLine( value + "\0" );
            Monitor.Exit( this );
            GotWrittenTo( null );
        }

    }
}
