using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DotnetStreams.Adapters.File;
using DotnetStreams.Adapters.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessageForwarder
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageReceiver<string, byte[]> _messageReceiver;
        private readonly IMessageSender<string, byte[]> _messageSender;

        public Worker(IMessageReceiver<string, byte[]> messageReceiver, IMessageSender<string, byte[]> messageSender, ILogger<Worker> logger)
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
                // get and forward the message to new topic
                var receivedMessage = _messageReceiver.ReceiveMessage();
                if (receivedMessage != null)
                {
                    if (FilterMessage(receivedMessage))
                    {
                        _messageSender.SendMessage(new Message<string, byte[]>() { Key = receivedMessage.Key, Value = receivedMessage.Value });
                    }
                }
                else
                {
                    _logger.LogInformation("MessageForwarder worker alive.");
                }
            }
        }

        private bool FilterMessage(ConsumeResult<string, byte[]> receivedMessage)
        {
            long fileChunk = GetFileChunk(receivedMessage);
            long numberOfChunks = GetNumberOfChunks(receivedMessage);
            return (fileChunk == numberOfChunks);
        }

        private long GetFileChunk(ConsumeResult<string, byte[]> receivedMessage)
        {
            return BitConverter.ToInt64(receivedMessage.Value, sizeof(long));
        }
        private long GetNumberOfChunks(ConsumeResult<string, byte[]> receivedMessage)
        {
            return BitConverter.ToInt64(receivedMessage.Value, sizeof(long) * 2);
        }
    }
}
