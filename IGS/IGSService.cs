using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using IGS.Server.IGS;

namespace IGS
{
    partial class IGSService : ServiceBase
    {
            public enum ServiceState
            {
                SERVICE_STOPPED = 0x00000001,
                SERVICE_START_PENDING = 0x00000002,
                SERVICE_STOP_PENDING = 0x00000003,
                SERVICE_RUNNING = 0x00000004,
                SERVICE_CONTINUE_PENDING = 0x00000005,
                SERVICE_PAUSE_PENDING = 0x00000006,
                SERVICE_PAUSED = 0x00000007,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ServiceStatus
            {
                public long dwServiceType;
                public ServiceState dwCurrentState;
                public long dwControlsAccepted;
                public long dwWin32ExitCode;
                public long dwServiceSpecificExitCode;
                public long dwCheckPoint;
                public long dwWaitHint;
            };

            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

            public IGSService()
            {
                InitializeComponent();


                eventLog = new EventLog();
                if (!EventLog.SourceExists("PointAndControlSource"))
                {
                    EventLog.CreateEventSource(
                        "PointAndControlSource", "PointAndControl_EventLog");
                }
                eventLog.Source = "PointAndControlSource";
                eventLog.Log = "PointAndControl_EventLog";
            }

            protected override void OnStart(string[] args)
            {

                igs = Initializer.InitializeIgs();
                eventLog.WriteEntry("Started and initialized Point and Control");
            }

            protected override void OnStop()
            {
                igs.Tracker.ShutDown();
                eventLog.WriteEntry("Stopped and Shut down Kinect Tracker");
            }
        }
    }
