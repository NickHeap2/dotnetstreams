using Confluent.Kafka;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Messaging
{
    public interface IMessageSender<K, V>
    {
        Task SendMessage(K key, V value);
        Task SendMessage(Message<K, V> message);
    }
}