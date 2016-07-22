namespace RpgSim.GameObjects
{
    public class Lock : GameObject
    {
        public bool Locked { get; set; }
        public string Key { get; set; }
        public int Strength { get; set; }

        public Lock(string id) : base(id)
        {
        }
    }
}