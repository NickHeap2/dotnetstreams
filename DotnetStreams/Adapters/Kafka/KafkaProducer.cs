using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Kafka
{
    public class KafkaProducer<K, V> : IKafkaProducer<K, V>
    {
        private readonly ILogger<KafkaProducer<K, V>> _logger;
        private readonly IProducer<K, V> _producer;
        private readonly IKafkaAdminClient _kafkaAdminClient;

        private bool topicInitialised = false;

        public KafkaProducer(IOptions<ProducerConfig> producerConfig, IKafkaAdminClient kafkaAdminClient, ILogger<KafkaProducer<K, V>> logger)
        {
            _logger = logger;
            _kafkaAdminClient = kafkaAdminClient;

            _logger.LogInformation("Building producer...");
            _producer = new ProducerBuilder<K, V>(producerConfig.Value)
                .SetLogHandler(OnLog)
                .SetErrorHandler(OnError)
                .Build();
        }

        private void OnError(IProducer<K, V> producer, Error error)
        {
            _logger.LogError(error.Reason); 
        }

        private void OnLog(IProducer<K, V> producer, LogMessage logMessage)
        {
            if (logMessage.Level == SyslogLevel.Warning)
            {
                _logger.LogWarning(logMessage.Message);
            }
            else if (logMessage.Level == SyslogLevel.Error)
            {
                _logger.LogError(logMessage.Message);
            }
            else
            {
                _logger.LogInformation(logMessage.Message);
            }
        }

        public async Task<bool> SendToKafka(string topicName, K key, V value)
        {
            if (!topicInitialised)
            {
                await _kafkaAdminClient.EnsureTopicExists(topicName);
                topicInitialised = true;
            }

            var deliveryResult = await _producer.ProduceAsync(topicName, new Message<K, V>() { Key = key, Value = value });
            return (deliveryResult.Status == PersistenceStatus.Persisted);
        }

        public async Task<bool> SendToKafka(string topicName, Message<K, V> message)
        {
            if (!topicInitialised)
            {
                await _kafkaAdminClient.EnsureTopicExists(topicName);
                topicInitialised = true;
            }

            var deliveryResult = await _producer.ProduceAsync(topicName, message);
            return (deliveryResult.Status == PersistenceStatus.Persisted);
        }

    }
}
