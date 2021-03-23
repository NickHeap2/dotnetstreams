using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Kafka
{
    public class KafkaConsumer<K, V> : IKafkaConsumer<K, V>
    {
        private readonly ILogger<KafkaConsumer<K, V>> _logger;
        private readonly IConsumer<K, V> _consumer;
        private readonly IKafkaAdminClient _kafkaAdminClient;

        private bool topicInitialised = false;

        public KafkaConsumer(IOptions<ConsumerConfig> consumerConfig, IKafkaAdminClient kafkaAdminClient, ILogger<KafkaConsumer<K, V>> logger)
        {
            _logger = logger;
            _kafkaAdminClient = kafkaAdminClient;

            _logger.LogInformation("Building consumer...");
            _consumer = new ConsumerBuilder<K, V>(consumerConfig.Value)
                .SetLogHandler(OnLog)
                .SetErrorHandler(OnError)
                .Build();
        }

        public ConsumeResult<K, V> ReceiveFromKafka(string topicName, TimeSpan timeout)
        {
            if (!topicInitialised)
            {
                _kafkaAdminClient.EnsureTopicExists(topicName);
                _consumer.Subscribe(topicName);
                topicInitialised = true;
            }

            try
            {
                var consumeResult = _consumer.Consume(timeout);
                return consumeResult;
            }
            catch (ConsumeException ce)
            {
                if (ce.Error.IsFatal)
                {
                    _logger.LogError(ce.Error.Reason);
                }
                else
                {
                    _logger.LogWarning(ce.Error.Reason);
                }
                return null;
            }
        }

        private void OnError(IConsumer<K, V> producer, Error error)
        {
            _logger.LogError(error.Reason);
        }

        private void OnLog(IConsumer<K, V> producer, LogMessage logMessage)
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

    }
}
