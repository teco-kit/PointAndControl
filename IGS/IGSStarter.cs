using IGS.Server.IGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IGS
{
    class IGSStarter
    {
        public bool igsRunning { get; set; }
        private CancellationTokenSource tokenSource { get; set; }
        private CancellationToken igsCancellationToken { get; set; }
        private Igs igs;


        public IGSStarter()
        {
            igsRunning = false;
            tokenSource = new CancellationTokenSource();
            igsCancellationToken = tokenSource.Token;
           
        }


        public async void igsStart()
        {
            igsRunning = true;

                await Task.Run(() =>
                {
                    try {
                        igs = Initializer.InitializeIgs();
                        igsRunning = true;
                        while (igs.isRunning)
                        {
                            igsCancellationToken.ThrowIfCancellationRequested();
                        }
                    } catch (OperationCanceledException)
                    {
                        igs.shutDown();
                        igsRunning = false;
                    }
                });
                igsRunning = false;
                Environment.Exit(1);
            

        }

        public void stopIGSConsoleCmd()
        {
            tokenSource.Cancel();

        }

    }
}
