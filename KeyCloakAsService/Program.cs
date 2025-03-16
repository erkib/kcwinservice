using System;
using System.Linq;

using KeyCloakAsService.Config;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if !DEBUG
using NReco.Logging.File;
#endif

namespace KeyCloakAsService
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            var runAsWindowsService = true;
            if (args.Length > 0 && args.Contains("--no-windows-service"))
                runAsWindowsService = false;

            var appArgs = args.Where(a => a != "--no-windows-service").ToArray();
            var builder = Host.CreateApplicationBuilder(appArgs);
#else
            var builder = Host.CreateApplicationBuilder(args);
#endif
            builder.Services.Configure<ProcessOptions>(builder.Configuration.GetSection(ProcessOptions.Process));
            builder.Services.AddSingleton<ProcessRunner>();
#if !DEBUG
            builder.Services.AddLogging(loggingBuilder =>
            {
                var loggingSection = builder.Configuration.GetSection("Logging");
                loggingBuilder.AddFile(loggingSection);
            });
#endif
#if DEBUG
            if (runAsWindowsService)
            {
#endif
                builder.Services.AddWindowsService(options =>
                {
                    options.ServiceName = "KeyCloakServiceWrapper";
                });
#if DEBUG
            }
#endif

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
