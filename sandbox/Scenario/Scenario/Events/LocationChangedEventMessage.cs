using RpgSim.EventMessaging;
using RpgSim.GameObjects;

namespace RpgSim.Events
{
    public class LocationChangedEventMessage : EventMessage
    {
        public Location Location { get; set; }

        public LocationChangedEventMessage(Location location)
            : base(MessageEnum.LocationChanged)
        {
            Location = location;
        }
    }

    public class PlayerMoveMessage : EventMessage
    {
        public string DestinationId { get; set; }

        public PlayerMoveMessage(string destinationId) 
            : base(MessageEnum.PlayerMoved)
        {
            DestinationId = destinationId;
        }
    }
}
