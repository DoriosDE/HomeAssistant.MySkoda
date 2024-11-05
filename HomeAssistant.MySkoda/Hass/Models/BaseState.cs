using HADotNet.Core.Models;
using System;
using System.Text.RegularExpressions;

namespace HomeAssistant.MySkoda.Hass.Models
{
    internal class BaseState : StateObject
    {
        protected string Prefix = "myskoda";

        public BaseState(string entityId, string suffix, DateTimeOffset? lastUpdated)
        {
            Attributes = new();

            entityId = Regex.Replace(entityId.ToLowerInvariant(), "[^a-z0-9]", "");
            base.EntityId = $"sensor.{Prefix}_{entityId}_{suffix}";

            LastUpdated = lastUpdated ?? DateTimeOffset.UtcNow;
            StateClass = "measurement";
        }

        public new DateTimeOffset LastUpdated
        {
            get { return Attributes["last_updated"] as DateTimeOffset? ?? DateTimeOffset.MinValue; }
            set { Attributes.Add("last_updated", value); }
        }

        public new string EntityId
        {
            get { return base.EntityId; }
        }

        public string UnitOfMeasurement
        {
            get { return Attributes["unit_of_measurement"]?.ToString() ?? null; }
            set { Attributes.Add("unit_of_measurement", value); }
        }

        public string FriendlyName
        {
            get { return Attributes["friendly_name"]?.ToString() ?? null; }
            set { Attributes.Add("friendly_name", value); }
        }

        public string StateClass
        {
            get { return Attributes["state_class"]?.ToString() ?? null; }
            set { Attributes.Add("state_class", value); }
        }

        public string DeviceClass
        {
            get { return Attributes["device_class"]?.ToString() ?? null; }
            set { Attributes.Add("device_class", value); }
        }
    }
}
