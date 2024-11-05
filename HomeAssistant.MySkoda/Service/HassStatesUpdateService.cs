using HomeAssistant.MySkoda.AppSettings;
using HomeAssistant.MySkoda.Hass;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySkodaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAssistant.MySkoda.Services
{
    internal class HassStatesUpdateService : BackgroundService
    {
        private readonly ILogger<HassStatesUpdateService> _logger;
        private readonly MySkodaConfig _mySkodaConfig;
        private readonly MySkodaClient _mySkodaClient;
        private readonly HassStatesUpdaterFactory _hassStatesUpdaterFactory;

        private List<HassStatesUpdater> _hassStatesUpdaters;

        public HassStatesUpdateService(ILogger<HassStatesUpdateService> logger, IOptions<MySkodaConfig> mySkodaConfigOptions, MySkodaClient mySkodaClient, HassStatesUpdaterFactory hassStatesUpdaterFactory)
        {
            _logger = logger;

            _mySkodaConfig = mySkodaConfigOptions.Value;
            _mySkodaClient = mySkodaClient;

            _hassStatesUpdaterFactory = hassStatesUpdaterFactory;
            _hassStatesUpdaters = new();
        }

        private async Task Initialize()
        {
            try
            {
                await _hassStatesUpdaterFactory.EnsureHassConnected();

                await _mySkodaClient.InitializeAsync(_mySkodaConfig.User, _mySkodaConfig.Password);

                var vehicleProviderTasks = _mySkodaConfig.Vins.Select(async vin =>
                {
                    var vehicleProvider = await _mySkodaClient.CreateVehicleProviderAsync(vin);
                    var statesUpdater = await _hassStatesUpdaterFactory.Create(vehicleProvider);
                    _hassStatesUpdaters.Add(statesUpdater);
                });

                await Task.WhenAll(vehicleProviderTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task UpdateStates()
        {
            await Task.WhenAll(_hassStatesUpdaters.Select(async statesUpdater => await statesUpdater.UpdateStates()));
        }

        #region BackgroundService

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"starting {nameof(HassStatesUpdateService)}");

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Initialize();

            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStates();

                await Task.Delay(10 * 1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"stopping {nameof(HassStatesUpdateService)}");

            await base.StopAsync(cancellationToken);
        }

        #endregion BackgroundService
    }
}