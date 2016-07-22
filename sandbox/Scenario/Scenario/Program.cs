namespace RpgSim
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game("Demo");
            var engine = new Engine(game);

            engine.ReloadCurrentLocation();
        }
    }
}
