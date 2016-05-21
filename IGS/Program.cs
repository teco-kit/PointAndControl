using System;

namespace PointAndControl
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
             
            PointAndControlStarter starter = new PointAndControlStarter();
            starter.igsStart();


            String command = "";
            while (!starter.pncRunning)
            {

            }
            while (starter.pncRunning)
            {
                Console.WriteLine(Properties.Resources.TypeConsoleCommand);

                command = Console.ReadLine();

                switch (command) {

                    case "stop":
                        starter.stopIGSConsoleCmd();
                        Console.WriteLine(Properties.Resources.ServerShutDown);
                        break;

                    default:
                        continue;
                }
            }
        }

    }
}
