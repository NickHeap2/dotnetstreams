using Confluent.Kafka;
using DotnetStreams.Adapters.File;
using DotnetStreams.Adapters.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Messaging
{
    public class MessageSender<K, V> : IMessageSender<K, V>
    {
        private readonly ILogger<FileSender> _logger;
        private readonly IKafkaProducer<K, V> _kafkaProducer;
        private readonly MessageSenderOptions _messageSenderOptions;

        private TimeSpan _receiveTimeout;

        public MessageSender(IOptions<MessageSenderOptions> messageSenderOptions, IKafkaProducer<K, V> kafkaProducer, ILogger<FileSender> logger)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _messageSenderOptions = messageSenderOptions.Value;
        }

        public async Task SendMessage(K key, V value)
        {
            await SendMessage(new Message<K, V>() { Key = key, Value = value });
        }
        public async Task SendMessage(Message<K, V> message)
        {
            _logger.LogInformation("Sending Key {Key} to Kafka...", message.Key);
            await _kafkaProducer.SendToKafka(_messageSenderOptions.TopicName, message);
        }
    }
}
