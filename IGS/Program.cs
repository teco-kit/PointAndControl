using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using IGS.Server.IGS;
using System.Threading;

namespace IGS
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
             
            IGSStarter starter = new IGSStarter();
            starter.igsStart();


            String command;

            while (starter.igsRunning)
            {
                //Console.WriteLine(Properties.Resources.TypeConsoleCommand);

                command = "";

                switch (command) {

                    case "stop":
                        starter.stopIGSConsoleCmd();
                        Console.WriteLine(Properties.Resources.ServerShutDown);
                        break;

                    default:
                        break;
                }
            }
        }

    }
}
