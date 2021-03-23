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
    public class MessageReceiver<K, V> : IMessageReceiver<K, V>
    {
        private readonly ILogger<FileSender> _logger;
        private readonly IKafkaConsumer<K, V> _kafkaConsumer;
        private readonly MessageReceiverOptions _messageReceiverOptions;

        private TimeSpan _receiveTimeout;

        public MessageReceiver(IOptions<MessageReceiverOptions> messageReceiverOptions, IKafkaConsumer<K, V> kafkaConsumer, ILogger<FileSender> logger)
        {
            _logger = logger;
            _kafkaConsumer = kafkaConsumer;
            _messageReceiverOptions = messageReceiverOptions.Value;

            _receiveTimeout = TimeSpan.FromMilliseconds(_messageReceiverOptions.ReceiveTimeout);
        }

        public ConsumeResult<K, V> ReceiveMessage()
        {
            return _kafkaConsumer.ReceiveFromKafka(_messageReceiverOptions.TopicName, _receiveTimeout);
        }

        public ConsumeResult<K, V> ReceiveFilteredMessage(Func<ConsumeResult<K, V>, bool> filterFunction)
        {
            
            var message = _kafkaConsumer.ReceiveFromKafka(_messageReceiverOptions.TopicName, _receiveTimeout);
            if (message != null
                && filterFunction(message))
            {
                return message;
            }
            else
            {
                return null;
            }
        }

    }
}
