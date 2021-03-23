using Confluent.Kafka;
using System;

namespace DotnetStreams.Adapters.Messaging
{
    public interface IMessageReceiver<K, V>
    {
        ConsumeResult<K, V> ReceiveMessage();
        ConsumeResult<K, V> ReceiveFilteredMessage(Func<ConsumeResult<K, V>, bool> filterFunction);
    }
}