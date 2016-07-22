using System.Collections.Generic;
using System.Linq;
using NLua;

namespace RpgSim.GameObjects
{
    public class Location : GameObject
    {
        public Location(string id) : base(id)
        {
            Items = new List<Item>();
            Containers = new List<Container>();
            Exits = new List<Exit>();
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public string GetDescription(Lua lua)
        {
            var id = Id.Split(':').Last();
            var description = lua[id + ".description"] as string ?? "";
            return description.EvaluateLuaSnippets(lua);
        }

        public ICollection<Item> Items { get; set; }
        public ICollection<Container> Containers { get; set; }
        public ICollection<Exit> Exits { get; set; }
    }
}