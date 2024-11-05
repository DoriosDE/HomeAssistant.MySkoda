using System;
using System.Globalization;

namespace HomeAssistant.MySkoda.Hass.Models
{
    internal class OdometerState : BaseState
    {
        public OdometerState(string entityId, double state, DateTimeOffset? lastUpdated = null)
            : base(entityId, "milage", lastUpdated)
        {
            State = string.Create(CultureInfo.InvariantCulture, $"{state:F2}");
            UnitOfMeasurement = "km";
            FriendlyName = $"Milage of {entityId}";
        }
    }
}
