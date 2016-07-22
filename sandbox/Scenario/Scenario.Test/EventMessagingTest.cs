using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpgSim.EventMessaging;

namespace Scenario.Test
{
    [TestClass]
    public class EventMessagingTest
    {
        [TestMethod]
        public void RegisterEventTest()
        {
            var messenger = EventMessagingSystem.Messenger;
            
            var received = false;
            messenger.Register(1000, message => { received = true; });
            messenger.Post(1000);

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void UnregisterEventTest()
        {
            var messenger = EventMessagingSystem.Messenger;
            var received = false;
            var callback = new Action<IEventMessage>(message => { received = true; });
            
            messenger.Register(1000, callback);
            messenger.Post(1000);
            Assert.IsTrue(received);

            received = false;
            
            messenger.Unregister(1000, callback);
            messenger.Post(1000);
            Assert.IsFalse(received);
        }
    }
}
