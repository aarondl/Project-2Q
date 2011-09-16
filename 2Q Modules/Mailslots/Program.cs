using System;
using System.Collections.Generic;
using System.Text;

namespace Mailslots {

    class Program {

        static unsafe void Main(string[] args) {

            //Create the file.
            IntPtr handle = Mailslots.CreateFile( @"c:\Bits\lawl.txt", (uint)(FileAccess.Generic_Read | FileAccess.Generic_Write),
                (uint)ShareMode.Disable, IntPtr.Zero, (uint)CreationDisposition.CreateAlways, 0, IntPtr.Zero );

            string haifriends = "lawl :D";
            

            IntPtr x;

            Mailslots.WriteFile( handle, new IntPtr( haifriends ), (uint)(haifriends.Length * 2), x, IntPtr.Zero );

            Console.WriteLine( "Bytes written: {0}", x.ToString() );

            Mailslots.CloseHandle( handle );

        }

    }

}
