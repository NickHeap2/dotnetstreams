using Confluent.Kafka;
using System;

namespace DotnetStreams.Adapters.Kafka
{
    public interface IKafkaConsumer<K, V>
    {
        ConsumeResult<K, V> ReceiveFromKafka(string topicName, TimeSpan timeout);
    }
}