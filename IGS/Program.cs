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


            String command = "";

            while (starter.igsRunning)
            {
                Console.WriteLine(Properties.Resources.TypeConsoleCommand);

                command = Console.ReadLine();

                switch (command) {

                    case "stop":
                        starter.stopIGSConsoleCmd();
                        Console.WriteLine(Properties.Resources.ServerShutDown);
                        break;
                    case "addDevice":
                        Console.WriteLine("Write Components as Promted - type break to stop");

                        String[] input = new string[] {"Type", "Id", "Name", "URL" };
                        int counter = 0;
                        bool broke = false;

                        while (counter < input.Length)
                        {
                            Console.WriteLine(input[counter]);
                            input[counter] = Console.ReadLine();
                            if (input[counter].Equals("break"))
                            {
                                broke = true;
                                break;
                            }
                                
                        }

                        if (!broke)
                        {
                            //call addDevice
                        }

                        break;
                        
                    default:
                        continue;
                }
            }
        }

    }
}
