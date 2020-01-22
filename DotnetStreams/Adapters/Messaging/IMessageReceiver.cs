using Confluent.Kafka;

namespace DotnetStreams.Adapters.Messaging
{
    public interface IMessageReceiver<K, V>
    {
        ConsumeResult<K, V> ReceiveMessage();
    }
}