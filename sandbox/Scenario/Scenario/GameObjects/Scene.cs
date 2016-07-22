using System.Collections.Generic;

namespace RpgSim.GameObjects
{
    public class Scene : GameObject
    {
        public Scene(string id) : base(id)
        {
            Locations = new List<Location>();
        }

        public string Comments { get; set; }
        public ICollection<Location> Locations { get; set; }
    }
}