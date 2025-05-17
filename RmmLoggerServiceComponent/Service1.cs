using System;
using System.IO;
using System.Management;
using System.ServiceProcess;
using System.Timers;

namespace RmmLoggerServiceComponent
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _timer;
        private string _logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + $@"\Temp\RmmLogger.log";
        private string serviceName = "RmmLoggerService";
        /// <summary>
        /// Append to the log file
        /// </summary>
        /// <param name="message">Message to add to the logs</param>
        private void AppendLog(string message)
        {
            File.AppendAllText(_logFilePath, $"[{DateTime.Now}] {message}{Environment.NewLine}");
        }
        public Service1()
        {
            this.ServiceName = serviceName;
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                AppendLog($"[{DateTime.Now}] | Unhandled exception: {e.ExceptionObject}");
            };
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)); // Ensure directory exists

            _timer = new System.Timers.Timer(10000); // 10 seconds
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            AppendLog($"[{DateTime.Now}] | RmmLogger Service started.");
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process");
                    foreach (ManagementObject process in searcher.Get())
                    {
                        string processName = process["Name"]?.ToString();
                        string pid = process["ProcessId"]?.ToString();
                        string owner = "Unknown";
                        try
                        {
                            string[] ownerInfo = new string[2];
                            process.InvokeMethod("GetOwner", ownerInfo);
                            owner = $"{ownerInfo[1]}\\{ownerInfo[0]}"; // Domain (or PC name)\User format (example: PC-01\User)
                            writer.WriteLine($"[{DateTime.Now}] | {processName} (PID: {pid}) | User: {owner}");
                        }
                        catch
                        {
                            writer.WriteLine($"[{DateTime.Now}] | {processName} (PID: {pid}) | User: Unknown");
                        }
                    }
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                AppendLog($"[{DateTime.Now}] | Error during process logging: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            _timer?.Stop();
            AppendLog($"[{DateTime.Now}] | Service has been stopped by the user.");
        }

        protected override void OnShutdown()
        {
            _timer?.Stop();
            AppendLog($"[{DateTime.Now}] | Service stopped due to system shutdown/restart.");
        }
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            AppendLog($"[{DateTime.Now}] | Power event received: {powerStatus}");
            return base.OnPowerEvent(powerStatus);
        }
    }
}
