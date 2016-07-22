namespace RpgSim.GameObjects
{
    public class Item : GameObject
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Item(string id) : base(id)
        {
        }
    }
}