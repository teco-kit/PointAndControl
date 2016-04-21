using PointAndControl.MainComponents;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PointAndControl
{
    class PointAndControlStarter
    {
        public bool pncRunning { get; set; }
        private CancellationTokenSource tokenSource { get; set; }
        private CancellationToken cancellationToken { get; set; }
        private PointAndControlMain pncMain;


        public PointAndControlStarter()
        {
            pncRunning = false;
            tokenSource = new CancellationTokenSource();
            cancellationToken = tokenSource.Token;
           
        }


        public async void igsStart()
        {
            pncRunning = true;

                await Task.Run(() =>
                {
                    try {
                        pncMain = Initializer.InitializeIgs();
                        pncRunning = true;
                        while (pncMain.isRunning)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    } catch (OperationCanceledException)
                    {
                        pncMain.shutDown();
                        pncRunning = false;
                    }
                });
                pncRunning = false;
                Environment.Exit(1);
            

        }

        public void stopIGSConsoleCmd()
        {
            tokenSource.Cancel();
        }

    }
}
