namespace Deullam.Credit.Inquiry.Challenge.Application.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string topic, string message);
    }
}
