using System;

namespace RpgSim.EventMessaging
{
    public interface IEventMessagingSystem
    {
        void Post(MessageEnum messageId);
        void Post(int messageId);
        void Post(IEventMessage message);
        void Register(MessageEnum messageId, Action<IEventMessage> callback);
        void Register(int messageId, Action<IEventMessage> callback);
        void Unregister(MessageEnum messageId, Action<IEventMessage> callback);
        void Unregister(int messageId, Action<IEventMessage> callback);
    }
}