using HADotNet.Core.Clients;
using Microsoft.Extensions.Logging;
using MySkodaSharp.Provider;
using System;
using System.Threading.Tasks;

namespace HomeAssistant.MySkoda.Hass
{
    internal class HassStatesUpdaterFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<HassStatesUpdaterFactory> _logger;
        private readonly ConfigClient _configClient;
        private readonly StatesClient _statesClient;

        public HassStatesUpdaterFactory(ILoggerFactory loggerFactory, ConfigClient configClient, StatesClient statesClient)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<HassStatesUpdaterFactory>();

            _configClient = configClient;
            _statesClient = statesClient;
        }

        public async Task EnsureHassConnected()
        {
            var result = await _configClient.CheckConfiguration();
            if (result == null || !string.IsNullOrWhiteSpace(result.Errors))
                throw new Exception(result?.Errors ?? "Failed to check Hass configuration");

            var config = await _configClient.GetConfiguration();

            var configInfo = $"""
                Version: {config.Version}
                LocationName: {config.LocationName}
                TimeZone: {config.TimeZone}
                """;

            _logger.LogInformation(configInfo);
        }

        public async Task<HassStatesUpdater> Create(VehicleProvider vehicleProvider)
        {
            var statesUpdater = new HassStatesUpdater(_loggerFactory, _statesClient, vehicleProvider);
            await statesUpdater.Initialize();
            return statesUpdater;
        }
    }
}
