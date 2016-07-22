using System.Collections.Generic;

namespace RpgSim.GameObjects
{
    public class Container : GameObject
    {
        public Container(string id) : base(id)
        {
            Items = new List<Item>();
        }

        public string Name { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
        public Lock Lock { get; set; }
        public bool Open { get; set; }

        public ICollection<Item> Items;
    }
}