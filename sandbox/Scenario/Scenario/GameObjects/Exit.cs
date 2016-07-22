namespace RpgSim.GameObjects
{
    public class Exit : GameObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DestinationId { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Exit(string id) : base(id)
        {
        }
    }
}