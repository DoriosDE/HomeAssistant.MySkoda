using HADotNet.Core.Clients;
using HomeAssistant.MySkoda.Hass.Adapters;
using Microsoft.Extensions.Logging;
using MySkodaSharp.Models;
using MySkodaSharp.Provider;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAssistant.MySkoda.Hass
{
    internal class HassStatesUpdater
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly StatesClient _statesClient;
        private readonly VehicleProvider _vehicleProvider;

        private VehicleResponse _vehiclesResponse;
        private ILogger _logger;
        private List<IAdapter> _adapters;

        public HassStatesUpdater(ILoggerFactory loggerFactory, StatesClient statesClient, VehicleProvider vehicleProvider)
        {
            _loggerFactory = loggerFactory;
            _statesClient = statesClient;
            _vehicleProvider = vehicleProvider;
        }

        public async Task Initialize()
        {
            //_vehiclesResponse = await _vehicleProvider.GetVehicleAsync();
            _vehiclesResponse = new VehicleResponse { Name = $"Enyaq" };

            _logger = _loggerFactory.CreateLogger($"{typeof(HassStatesUpdater).FullName}[\"{_vehiclesResponse.Name}\"]");

            _adapters = new()
            {
                new OdometerAdapter(_vehiclesResponse.Name, _vehicleProvider),
            };
        }

        public async Task UpdateStates()
        {
            _logger.LogDebug("Updating states...");

            var adapterTasks = _adapters.Select(async adapter =>
            {
                var stateObjects = await adapter.GetStateObjectsAsync();
                var stateTasks = stateObjects.Select(async stateObject =>
                {
                    await _statesClient.SetState(stateObject.EntityId, stateObject.State, stateObject.Attributes);

                    _logger.LogTrace($"{stateObject.EntityId} => {stateObject.State}");
                });
                await Task.WhenAll(stateTasks);
            });

            await Task
                .WhenAll(adapterTasks)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception.Message);
                    }
                    else
                    {
                        _logger.LogDebug("Updating states have completed successfully.");
                    }
                });
        }
    }
}