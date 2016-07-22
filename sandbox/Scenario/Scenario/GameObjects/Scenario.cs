using System.Collections.Generic;

namespace RpgSim.GameObjects
{
    public class Scenario : GameObject
    {
        public string Name { get; set; }
        public ICollection<Scene> Scenes { get; set; }

        public Scenario(string id) : base(id)
        {
            Scenes = new List<Scene>();
        }
    }
}