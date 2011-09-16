using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Project2Q.Core;

namespace Project2Q.Qonsole {

    class BATBotWrapper {

        static void Main(string[] args) {

            DateTime dt = DateTime.Now;
            Project2QService qq;
            Thread t;
            qq = new Project2QService( "../../../Config.xml", Console.In, Console.Out );
            t = new Thread( new ParameterizedThreadStart( qq.OnStart ) );
            t.Start( null );

            string input;
            do {
                input = Console.ReadLine();
                if ( input.StartsWith( "send " ) ) {
                    input = input.Remove( 0, "send ".Length );
                    Console.WriteLine("Sending {0}: {1}", input.Split( ' ' )[0] , input.Remove( 0, 2 ) );
                    qq.SendMessage( int.Parse( input.Split( ' ' )[0] ) , input.Remove( 0, 2 ) );
                }
                if ( input.Equals( "uptime" ) ) {
                    TimeSpan up = DateTime.Now - dt;
                    Console.WriteLine( up.ToString() );
                }
            } while ( !input.Equals("quit") );

            qq.OnStop();

#if DEBUG
            Console.ReadKey(true);
#endif
        }

    }

}
