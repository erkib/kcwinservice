using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyCloakAsService
{
    public class Worker(ILogger<Worker> logger, ProcessRunner runner) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Server starting");

            try
            {
                StartExternalApp();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!runner.IsBusy)
                        throw new ApplicationException("Terminated.");

                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Shutdown requested.");

                StopExternalApp();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "{Message}", ex.Message);

                if (ex.GetType() == typeof(ApplicationException))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                        logger.LogError(runner.Process.StandardError.ReadToEnd());
                }

                StopExternalApp();

                Environment.Exit(1);
            }
            finally
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug("Server shutdown done.");
            }
        }

        private void StartExternalApp()
        {
            runner.RunWorkerAsync();

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Runner started");
        }

        private void StopExternalApp()
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug("Killing process");

                runner.Stop();
            }
            catch (Exception e)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(e, "Process.Kill");
            }
        }
    }
}
