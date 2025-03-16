using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using KeyCloakAsService.Config;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyCloakAsService
{
    public class ProcessRunner : BackgroundWorker
    {
        private readonly ProcessOptions options;
        private readonly ILogger<ProcessRunner> logger;

        public Process Process { get; } = new();

        public ProcessRunner(ILogger<ProcessRunner> logger, IOptions<ProcessOptions> config)
        {
            WorkerSupportsCancellation = true;

            options = config.Value;
            this.logger = logger;
        }

        public void Stop()
        {
            Process.ErrorDataReceived -= Process_ErrorDataReceived;
            Process.OutputDataReceived -= Process_OutputDataReceived;

            Process.Kill();
            CancelAsync();

            var p = Process.GetProcesses();
            if (p != null && p.Length > 0)
            {
                foreach (var proc in p)
                {
                    if (proc.ProcessName == "java")
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("Process: {0}, {1}, killing it...", proc.Id, proc.ProcessName);
                        proc.Kill();
                        break;
                    }
                }
            }

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Kill done");
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Starting external process.");

            Process.StartInfo.FileName = options.Path;
            if (options.Args.Count > 0)
                Process.StartInfo.Arguments = string.Join(" ", options.Args);
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.WorkingDirectory = Path.GetDirectoryName(options.Path);
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.OutputDataReceived += Process_OutputDataReceived;
            var started = Process.Start();

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Process {Id} started: {Success}", Process.Id, started);

            Process.WaitForExit();

            base.OnDoWork(e);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(e.Data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Process.Dispose();

            base.Dispose(disposing);
        }
    }
}
