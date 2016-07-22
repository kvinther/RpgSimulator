using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RpgSim.EventMessaging
{
    public class EventMessagingSystem  : IEventMessagingSystem
    {
        // Singleton instance.
        public static IEventMessagingSystem Messenger
        {
            get { return _messenger ?? (_messenger = new EventMessagingSystem()); }
        }

        private readonly Dictionary<int, List<Action<IEventMessage>>> _listenersDictionary;
        private readonly List<Action<IEventMessage>> _broadcastListeners;
        private static EventMessagingSystem _messenger;

        private EventMessagingSystem()
        {
            _listenersDictionary = new Dictionary<int, List<Action<IEventMessage>>>();
            _broadcastListeners = new List<Action<IEventMessage>>();
        }

        public void Post(MessageEnum messageId)
        {
            Post((int)messageId);
        }

        public void Post(int messageId)
        {
            Post(new EventMessage(messageId));
        }

        public void Post(IEventMessage message)
        {
            var messageId = message.MessageId.ToString();
            MessageEnum test;
            if (Enum.TryParse(message.MessageId.ToString(), out test))
                messageId = test.ToString();

            Debug.WriteLine(string.Format("MessagePosted ({0}, {1})", messageId, message.GetType().Name));

            // First active the registered "broadcast" call back methods.
            foreach (var callback in _broadcastListeners)
                callback(message);

            // Find registered listeners for the message, if any.
            List<Action<IEventMessage>> listeners;
            if (!_listenersDictionary.TryGetValue(message.MessageId, out listeners))
                return;

            // For each listeners, activate the registered call back method.
            foreach (var callback in listeners)
                callback(message);
        }

        public void Register(MessageEnum messageId, Action<IEventMessage> callback)
        {
            Register((int) messageId, callback);
        }

        public void Register(int messageId, Action<IEventMessage> callback)
        {
            // A messageId of '0' (zero) means that the listener wants all messages.
            if (messageId == 0)
            {
                _broadcastListeners.Add(callback);
                return;
            }

            // Check if messageId has any previous registrations. If not, create a new list.
            if (!_listenersDictionary.ContainsKey(messageId))
                _listenersDictionary[messageId] = new List<Action<IEventMessage>>();

            // Check if the same callback has already been registered. If so, ignore.
            if (_listenersDictionary[messageId].Any(x => x == callback))
                return;

            // Add the call back to the listeners list.
            _listenersDictionary[messageId].Add(callback);
        }

        public void Unregister(MessageEnum messageId, Action<IEventMessage> callback)
        {
            Unregister((int)messageId, callback);
        }

        public void Unregister(int messageId, Action<IEventMessage> callback)
        {
            // A messageId of '0' (zero) means that the listener no longer wants all messages.
            if (messageId == 0)
            {
                _broadcastListeners.Remove(callback);
                return;
            }

            // Find registered listeners for the message, if any.
            List<Action<IEventMessage>> listeners;
            if (!_listenersDictionary.TryGetValue(messageId, out listeners))
                return;

            // Remove the callback from the listerner list.
            _listenersDictionary[messageId].Remove(callback);
        }
    }
}