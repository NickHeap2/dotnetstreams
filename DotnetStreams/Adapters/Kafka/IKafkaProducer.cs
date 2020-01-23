using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Kafka
{
    public interface IKafkaProducer<K, V>
    {
        Task<bool> SendToKafka(string topicName, K key, V value);
        Task<bool> SendToKafka(string topicName, Message<K, V> message);
    }
}
