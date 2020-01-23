using Confluent.Kafka;

namespace DotnetStreams.Adapters.Messaging
{
    public interface IMessageSender<K, V>
    {
        void SendMessage(K key, V value);
        void SendMessage(Message<K, V> message);
    }
}