using Common;
using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace UptimeService
{
    public partial class UptimeService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

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

        private const int TIMER_INTERVALL = 60000;
        private readonly BackgroundWorker _pipeServerWorker;
        private bool _terminate = false;

        public UptimeService()
        {
            InitializeComponent();

            UptimeServiceSettings.Default.Reload();

            _pipeServerWorker = new BackgroundWorker();
            _pipeServerWorker.DoWork += _pipeServerWorker_DoWork;
            _pipeServerWorker.RunWorkerAsync();

            AutoLog = false;
    
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("UptimeSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "UptimeSource", "UptimeLog");
            }
            eventLog.Source = "UptimeSource";
            eventLog.Log = "UptimeLog";
        }

        void _pipeServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_terminate)
            {
                var pipeServer = new NamedPipeServerStream("uptimepipe", PipeDirection.Out, 2); 

                pipeServer.WaitForConnection();

                var stream = new StreamString(pipeServer);

                var t = CheckUptime();
                var message = "Current uptime: " + t.GetFormattedUptime() + Environment.NewLine +
                    "Record uptime: " + UptimeServiceSettings.Default.MaxUptime.GetFormattedUptime();

                stream.WriteString(message);
                pipeServer.Close();
            }            
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            //ServiceStatus serviceStatus = new ServiceStatus();
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            //serviceStatus.dwWaitHint = 100000;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("Updatime service started.");

            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = TIMER_INTERVALL;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            // Update the service state to Start Pending.
            //ServiceStatus serviceStatus = new ServiceStatus();
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            //serviceStatus.dwWaitHint = 100000;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("Updatime service stopped.");
            _terminate = true;

            // Update the service state to Stopped.
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            //SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnContinue()
        {
            eventLog.WriteEntry("Updatime service continued.");  
            base.OnContinue();
        }

        private TimeSpan CheckUptime()
        {
            var record = UptimeServiceSettings.Default.MaxUptime;
            var t = Uptime.GetUptime2();

            if (t > record)
            {
                UptimeServiceSettings.Default.MaxUptime = t;
                UptimeServiceSettings.Default.Save();
                record = t;
            }

            return t;
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            var t = CheckUptime();
            eventLog.WriteEntry("Current uptime: " + t.GetFormattedUptime() + 
                "\nRecord uptime: " + UptimeServiceSettings.Default.MaxUptime.GetFormattedUptime());
        }
    }
}
