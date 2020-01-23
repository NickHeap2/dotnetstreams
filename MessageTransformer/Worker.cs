using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotnetStreams.Adapters.File;
using DotnetStreams.Adapters.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessageTransformer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageReceiver<string, byte[]> _messageReceiver;
        private readonly IMessageSender<string, string> _messageSender;

        public Worker(IMessageReceiver<string, byte[]> messageReceiver, IMessageSender<string, string> messageSender, ILogger<Worker> logger)
        {
            _logger = logger;
            _messageReceiver = messageReceiver;
            _messageSender = messageSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for messages...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var receivedMessage = _messageReceiver.ReceiveMessage();
                if (receivedMessage != null)
                {
                    _messageSender.SendMessage(TransformMessage(receivedMessage));
                }
                else
                {
                    _logger.LogInformation("MessageTransformer worker alive.");
                }
            }
        }

        private Message<string, string> TransformMessage(ConsumeResult<string, byte[]> receivedMessage)
        {
            long fileSize = GetFileSize(receivedMessage);

            return new Message<string, string>()
            {
                Key = receivedMessage.Key,
                Value = $"{{ \"receivedFilename\": \"{receivedMessage.Key}\", \"fileSize\": {fileSize} }}"
            };
        }

        private long GetFileSize(ConsumeResult<string, byte[]> receivedMessage)
        {
            return BitConverter.ToInt64(receivedMessage.Value, 0);
        }
    }
}
