using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scenario.GameObjects;
using Lua = NLua.Lua;

namespace Scenario.Test
{
    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void LuaFiddle()
        {
            var lua = GetLua();
            lua.DoString("look()");
            lua.DoString("get('key')");
            lua.DoString("look()");
        }

        private Lua GetLua()
        {
            var lua = new Lua();
            lua.RegisterFunction("write", this, GetType().GetMethod("Print"));
            lua.RegisterFunction("look", this, GetType().GetMethod("Look"));
            lua.RegisterFunction("get", this, GetType().GetMethod("GetItem"));
            lua.RegisterFunction("global", this, GetType().GetMethod("GetGlobal"));
            lua.RegisterFunction("getCurrentRoom", this, GetType().GetMethod("GetCurrentLocation"));
            return lua;
        }

        public void GetItem(string itemName)
        {
            var room = GetCurrentLocation();
            var item =
                room.Items.SingleOrDefault(x => x.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

            if (item != null)
            {
                Print(string.Format("You pick up the {0}.", itemName));
                room.Items.Remove(item);
            }
            else
            {
                Print(string.Format("You can't get the {0}.", itemName));
            }
            Print("");
        }

        public string GetGlobal(string identifier)
        {
            var gameTime = new DateTime(1924, 2, 20, 14, 0, 0);

            var timeOfDay = gameTime.Hour < 6 || gameTime.Hour > 22
                ? "night" : "day";

            var globals = new Dictionary<string, string> { { "g:timeofday", timeOfDay } };

            string value;
            globals.TryGetValue(identifier, out value);
            return value;
        }

        public void Look()
        {
            var lua = GetLua();
            var room = GetCurrentLocation();
            var text = room.Description.EvaluateLuaSnippets(lua) + Environment.NewLine;
            room.Containers.ToList().ForEach(x => text += x.Description.EvaluateLuaSnippets(lua) + Environment.NewLine);
            room.Items.ToList().ForEach(x => text += x.Description.EvaluateLuaSnippets(lua) + Environment.NewLine);
            Print(text);
        }

        public void Print(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
        }

        private Scene _myScene;
        public Location GetCurrentLocation()
        {
            if (_myScene == null)
                _myScene = GetDemoScene();
            return _myScene.Locations.First();
        }

        [TestMethod]
        public void DeserializeScene()
        {
            var path = @"C:\Users\klv\Dropbox\w\git\rpgsimulator\scenario\AnExample\scenes\scene2.json";
            var json = File.ReadAllText(path);
            json.FromJsonToObject<Scene>();
            Assert.IsTrue(true);
        }

        private Scene GetDemoScene()
        {
            return new Scene("urn:scene:hauntedhouse")
            {
                Comments = "The haunted house."
            }.WithLocation(new Location("urn:location:hountedhouse:driveway")
            {
                Name = "A gravel driveway in front of a mansion.",
                Description = @"A large circular driveway with a odd-looking fountain on a small lawn.
The weed is slowly taking over the place. The sky is <lua>if global('g:timeofday') == 'day' then return 'blue' else return 'pitch black' end</lua>.",

                Exits = new List<Exit>
                {
                    new Exit("")
                    {
                        Name = "North",
                        Description = "The mansion entrance.",
                        DestinationId = "urn:location:hountedhouse:entrance"
                    },
                    new Exit("")
                    {
                        Name = "South",
                        Description = "The road back towards the village.",
                        DestinationId = "urn:location:theroad:endoftheroad"
                    }
                }
            }.WithContainer(new Container("urn:container:mailbox")
            {
                Name = "mailbox",
                Description = "An old rusty mailbox is standing near the stairs to the entrance. It looks very old. The flag is raised.",

            }.WithItem(new Item("urn:item:letter")
            {
                Name = "letter",
                Description = "The letter is dated January 1st 1897 and addressed to an 'Uncle Timothy'.",
            })
            )
            .WithItem(new Item("urn:smallkey")
            {
                Name = "key",
                Description = "A small key is lying next to the mailbox."
            }))
            ;
        }

        [TestMethod]
        public void SerializeScene()
        {
            var scene = GetDemoScene();

            var json = scene.ToJson();
            Assert.IsTrue(true);
        }
    }
}
