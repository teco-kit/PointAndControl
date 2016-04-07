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


        public IGSStarter(Igs igs)
        {
            igsRunning = false;
            tokenSource = new CancellationTokenSource();
            igsCancellationToken = tokenSource.Token;
            this.igs = igs;
        }


        public async void igsStart()
        {
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
            }
            catch (OperationCanceledException e)
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
