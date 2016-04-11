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
            try
            {
                await Task.Run(() =>
                {
                    igs = Initializer.InitializeIgs();
                    igsRunning = true;
                    while (igs.isRunning)
                    {
                        igsCancellationToken.ThrowIfCancellationRequested();
                    }
                });
                igsRunning = false;
                Environment.Exit(1);
            }
            catch (OperationCanceledException)
            {
                igs.shutDown();
                igsRunning = false;
            }
        }

        public void stopIGSConsoleCmd()
        {
            tokenSource.Cancel();

        }

    }
}
