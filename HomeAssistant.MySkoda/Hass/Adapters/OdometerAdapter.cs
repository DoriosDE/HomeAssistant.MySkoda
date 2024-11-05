using HADotNet.Core.Models;
using HomeAssistant.MySkoda.Hass.Models;
using MySkodaSharp.Provider;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAssistant.MySkoda.Hass.Adapters
{
    internal class OdometerAdapter : IAdapter
    {
        private readonly string _entityId;
        private readonly VehicleProvider _vehicleProvider;

        public OdometerAdapter(string entityId, VehicleProvider vehicleProvider)
        {
            _entityId = entityId;
            _vehicleProvider = vehicleProvider;
        }

        public async Task<List<StateObject>> GetStateObjectsAsync()
        {
            return new()
            {
                new OdometerState(_entityId, 12345),
            };
        }
    }
}
