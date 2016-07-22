using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLua;
using RpgSim.EventMessaging;
using RpgSim.Events;
using RpgSim.GameObjects;

namespace RpgSim
{
    public class Engine
    {
        private readonly Game _game;
        private readonly Lua _lua;
        private readonly IEventMessagingSystem _messenger;

        public Engine(Game game)
        {
            _game = game;
            _messenger = EventMessagingSystem.Messenger;
            _lua = new Lua();

            RegisterEvents();
            RegisterLuaFunctions(this);
            
            RunEngineScripts();
            RunScenarioInitScripts();
        }

        private void RunEngineScripts()
        {
            _lua.LoadCLRPackage();            
            LuaRunner.RunLuaScriptsInFolder(_lua, LuaRunner.GetEngineLuaScriptsFolder());
        }

        private void RunScenarioInitScripts()
        {
            LuaRunner.RunLuaScriptsInFolder(_lua, LuaRunner.GetScenarioLuaScriptsFolder(_game.GameName));
        }

        private void RegisterEvents()
        {
            // Register with id=0, means all events ("broadcast").
            _messenger.Register(0, OnEvent);
        }

        public void OnEvent(IEventMessage message)
        {
            var targetType = message.GetType();
            var methods = GetType().GetMethodsBySig(typeof(void), targetType).ToArray();
            foreach (var method in methods)
                method.Invoke(this, new object[] { message });
        }

        public void HandleEvent(EventMessage message)
        {
            switch (message.MessageId)
            {
                case (int)MessageEnum.ReloadCurrentLocation:
                    ReloadCurrentLocation();
                    break;
            }
        }

        public void PlayerMove(PlayerMoveMessage message)
        {
            _game.SetCurrentLocation(message.DestinationId);
            ReloadCurrentLocation();
        }

        public void ReloadCurrentLocation()
        {
            var location = _game.GetCurrentLocation();
            InitLocation(location);
            _messenger.Post(new LocationChangedEventMessage(location));
        }

        public void InitLocation(Location location)
        {
            var lua = GetLuaFromLocationUrn(location.Id);
            if (!string.IsNullOrWhiteSpace(lua))
            {
                LuaRunner.DoString(_lua, lua);
                location.Description = location.GetDescription(_lua);
                var id = location.Id.Split(':').Last();
                var table2 = _lua.GetTable(id + ".exits");

                location.Exits.Clear();

                int exitNumber = 1;
                foreach (KeyValuePair<object, object> entry in table2)
                {
                    var tableId = string.Format("{0}.{1}.{2}", id, "exits", entry.Key);
                    var t = _lua.GetTable(tableId);
                    var dict = new Dictionary<string, string>();
                    foreach (KeyValuePair<object, object> exitTable in t)
                        dict.Add(exitTable.Key.ToString(), exitTable.Value.ToString());
                    var exit = new Exit(string.Format("urn:location:{0}:exits:exit{1}",id,exitNumber++))
                    {
                        Name = dict["name"],
                        DestinationId = dict["destination"],
                        Description = dict["description"],
                    };

                    location.Exits.Add(exit);
                }
                
            }
            else
            {
                location.Description = location.Description.EvaluateLuaSnippets(_lua);
            }
        }

        private void RegisterLuaFunctions(object obj)
        {
            foreach (var method in obj.GetType().GetMethods())
            {
                var luaAttribute = method.GetCustomAttributes(typeof(LuaRegisterAttribute), false);
                foreach (LuaRegisterAttribute attribute in luaAttribute)
                {
                    Debug.WriteLine("LUA: Register function: {0} -> {1}",
                        attribute.FunctionName, method.Name);
                    _lua.RegisterFunction(attribute.FunctionName, obj, method);
                }
            }
        }

        public string GetLuaFromLocationUrn(string urn)
        {
            var index = urn.IndexOf("location", StringComparison.InvariantCultureIgnoreCase);
            var parts = urn.Substring(index).Split(':');
            var scene = parts[1];
            var location = parts[2];
            var basePath = LuaRunner.GetScenarioLuaBaseFolder(_game.GameName);
            var path = Path.Combine(basePath, "scenes", scene, location) + ".lua";
            if (!File.Exists(path))
            {
                Debug.WriteLine("LUA: Lua file not found for location: {0}, Path: {1}",
                    urn, path);
                return string.Empty;
            }

            return File.ReadAllText(path);
        }

        [LuaRegister("GetGlobal")]
        public string GetGlobal(string identifier)
        {
            string value;
            _game.Globals.TryGetValue(identifier, out value);
            return value ?? "";
        }

        [LuaRegister("SetGlobal")]
        public void SetGlobal(string identifier, string value)
        {
            _game.Globals[identifier] = value;
        }
    }
}