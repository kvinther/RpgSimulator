using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpgSim;
using RpgSim.GameObjects;

namespace Scenario.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LuaFiddle()
        {
            var game = new Game("Demo");
            var engine = new Engine(game);
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
