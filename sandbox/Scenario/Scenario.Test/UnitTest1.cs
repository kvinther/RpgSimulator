using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
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
            lua.DoString("lookin('mailbox')");
            lua.DoString("open('mailbox')");
            lua.DoString("lookin('mailbox')");
            lua.DoString("getfrom('mailbox', 'letter')");
            lua.DoString("lookin('mailbox')");
            lua.DoString("look()");
        }

        private Lua GetLua()
        {
            var lua = new Lua();
            lua.RegisterFunction("write", this, GetType().GetMethod("Print"));
            lua.RegisterFunction("look", this, GetType().GetMethod("Look"));
            lua.RegisterFunction("lookin", this, GetType().GetMethod("LookIn"));
            lua.RegisterFunction("get", this, GetType().GetMethod("GetItem"));
            lua.RegisterFunction("open", this, GetType().GetMethod("OpenContainer"));
            lua.RegisterFunction("getfrom", this, GetType().GetMethod("GetFrom"));
            lua.RegisterFunction("global", this, GetType().GetMethod("GetGlobal"));
            lua.RegisterFunction("getCurrentRoom", this, GetType().GetMethod("GetCurrentLocation"));
            return lua;
        }

        public void GetFrom(string containerName, string itemName)
        {
            Print(string.Format(">>> getfrom {0} {1}",  containerName, itemName));

            var room = GetCurrentLocation();
            var container =
                room.Containers.SingleOrDefault(
                    x => x.Name.Equals(containerName, StringComparison.InvariantCultureIgnoreCase));
            if (container == null)
            {
                Print("No such thing to look in.");
                return;
            }

            var item = container.Items.SingleOrDefault(
                x => x.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

            if (item == null)
            {
                Print(string.Format("The {0} does not contain '{1}'.", containerName, itemName));
                return;
            }

            Print(string.Format("You get the {0} from the {1}.", itemName, containerName));
            container.Items.Remove(item);

            Print("");
        }

        public void LookIn(string containerName)
        {
            Print(">>> lookin " + containerName);
            var room = GetCurrentLocation();
            var container =
                room.Containers.SingleOrDefault(
                    x => x.Name.Equals(containerName, StringComparison.InvariantCultureIgnoreCase));
            if (container != null)
            {
                if (container.Open)
                {
                    if (container.Items.Any())
                    {
                        Print(string.Format("The {0} contains:", containerName));
                        foreach (var item in container.Items)
                            Print(item.Description.EvaluateLuaSnippets(GetLua()));
                    }
                    else
                    {
                        Print(string.Format("The {0} is empty.", containerName));
                    }
                }
                else
                    Print(string.Format("The {0} is closed.", containerName));
            }
            else
            {
                Print("No such thing to look in.");
            }
            Print("");
        }

        public void OpenContainer(string containerName)
        {
            Print(">>> open " + containerName);

            var room = GetCurrentLocation();
            var container =
                room.Containers.SingleOrDefault(
                    x => x.Name.Equals(containerName, StringComparison.InvariantCultureIgnoreCase));
            if (container != null)
            {
                if (container.Open)
                    Print(string.Format("The {0} is already open.", containerName));
                else
                {
                    Print(string.Format("You open the {0}.", containerName));
                    container.Open = true;
                }
            }
            else
            {
                Print("No such thing to open.");
            }
            Print("");
        }

        public void GetItem(string itemName)
        {
            Print(">>> get " + itemName);
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
            Print(">>> look");

            var lua = GetLua();
            var room = GetCurrentLocation();
            var text = room.Description.EvaluateLuaSnippets(lua) + Environment.NewLine;
            room.Containers.ToList().ForEach(x => text += x.Description.EvaluateLuaSnippets(lua) + Environment.NewLine);
            room.Items.ToList().ForEach(x => text += x.Description.EvaluateLuaSnippets(lua) + Environment.NewLine);
            room.Exits.ToList().ForEach(x =>
            {
                text += string.Format("{0} to {1}", x.Name, x.Description.EvaluateLuaSnippets(lua).LowerCaseFirst() + Environment.NewLine);
            });
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
                Description = "A large circular driveway with an odd-looking fountain on a small lawn in its center. " +
                              "The weed is slowly taking over the place. " +
                              "Above the large old house the sky is <lua>if global('g:timeofday') == 'day' then return 'a pale blue' else return 'pitch black' end</lua>.",

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
                Open = false

            }.WithItem(new Item("urn:item:letter")
            {
                Name = "letter",
                Description = "An unread letter. The letter is dated January 1st 1897 and addressed to an 'Uncle Timothy'.",
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
