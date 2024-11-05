using HADotNet.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAssistant.MySkoda.Hass.Adapters
{
    internal interface IAdapter
    {
        Task<List<StateObject>> GetStateObjectsAsync();
    }
}