using HADotNet.Core;
using HADotNet.Core.Clients;
using HomeAssistant.MySkoda.AppSettings;
using HomeAssistant.MySkoda.Hass;
using HomeAssistant.MySkoda.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySkodaSharp;
using NLog;
using NLog.Extensions.Logging;
using System;

namespace HomeAssistant.MySkoda
{
    internal class Program
    {
        private static ILogger<Program> _logger;

        public static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("MainLogger");
            try
            {
                logger.Info("Init method \"Main\".");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddNLog(new NLogProviderOptions { RemoveLoggerFactoryFilter = false });
                })
                .ConfigureServices((context, services) =>
                {
                    var versionInfo = $"""
                        Version: {ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}{ThisAssembly.Git.SemVer.DashLabel}
                        CommitDate: {ThisAssembly.Git.CommitDate}
                        BaseTag: {ThisAssembly.Git.BaseTag}
                        Commit: {ThisAssembly.Git.Commit}
                        Branch: {ThisAssembly.Git.Branch}
                        """;

                    _logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
                    _logger.LogInformation(versionInfo);

                    services.AddOptions();
                    services.Configure<MySkodaConfig>(context.Configuration.GetSection(nameof(MySkodaConfig)));
                    services.Configure<HassConfig>(context.Configuration.GetSection(nameof(HassConfig)));

                    var hassConfig = services.BuildServiceProvider().GetService<IOptions<HassConfig>>().Value;

                    ClientFactory.Initialize(hassConfig.InstanceAddress, hassConfig.ApiKey);

                    services.AddScoped(_ => ClientFactory.GetClient<ConfigClient>());
                    services.AddScoped(_ => ClientFactory.GetClient<StatesClient>());

                    services.AddSingleton<MySkodaClient>();
                    services.AddSingleton<HassStatesUpdaterFactory>();
                    services.AddHostedService<HassStatesUpdateService>();
                })
                .UseWindowsService()
                .UseSystemd();
    }
}