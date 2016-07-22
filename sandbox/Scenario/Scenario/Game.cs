using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RpgSim.GameObjects;

namespace RpgSim
{
    public class Game
    {
        public ICollection<Scene> Scenes { get; private set; }
        public ICollection<Location> Locations { get; private set; }
        public string GameName { get; private set; }
        public Dictionary<string, string> Globals = new Dictionary<string, string>();
        
        readonly Dictionary<string, string> _state;

        public Game(string gameName, Dictionary<string, string> state = null)
        {
            GameName = gameName;
            _state = state;
            
            Scenes = LoadScenes(GameName);
            Locations = Scenes.SelectMany(x => x.Locations).ToList();

            _state = state ?? new Dictionary<string, string>();

            SetState(Urns.CurrentLocation, Scenes.First().Locations.First().Id);

        }

        public void SetState(string key, string value)
        {
            _state[key] = value;
        }

        public string GetState(string key)
        {
            string value;
            return !_state.TryGetValue(key, out value) ? string.Empty : value;
        }

        static List<Scene> LoadScenes(string gameName)
        {
            var path = Path.Combine(LuaRunner.GetScenarioSaveFolder(gameName), "scenes");

            return Directory.GetFiles(path, "*.json", SearchOption.AllDirectories)
                .Select(x => File.ReadAllText(x).FromJsonToObject<Scene>())
                .ToList();
        }

        public Location GetCurrentLocation()
        {
            return FindLocation(GetState(Urns.CurrentLocation));
        }

        public Location FindLocation(string id)
        {
            return Locations.SingleOrDefault(x => x.Id == id);
        }

        public Scene FindScene(string id)
        {
            return Scenes.SingleOrDefault(x => x.Id == id);
        }

        public void SetCurrentLocation(string locationId)
        {
            if (FindLocation(locationId) == null)
                throw new ApplicationException("Unknown location: " + locationId)
                    .WithData("locationId", locationId);
            SetState(Urns.CurrentLocation, locationId);
        }
    }
}
