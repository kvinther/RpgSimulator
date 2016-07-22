namespace RpgSim.EventMessaging
{
    public interface IEventMessage
    {
        int MessageId { get; set; }
    }

    public class EventMessage : IEventMessage
    {
        public int MessageId { get; set; }

        public EventMessage(int messageId)
        {
            MessageId = messageId;
        }
        public EventMessage(MessageEnum messageId)
            : this((int) messageId)
        {
        }
    }
}