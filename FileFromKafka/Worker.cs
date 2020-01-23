using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotnetStreams.Adapters.File;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileFromKafka
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFileReceiver _fileReceiver;

        public Worker(IFileReceiver fileReceiver, ILogger<Worker> logger)
        {
            _logger = logger;
            _fileReceiver = fileReceiver;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for files...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var receivedAChunk = _fileReceiver.ReceiveFileChunk();
                if (!receivedAChunk)
                {
                    _logger.LogInformation("FileFromKafka worker alive.");
                }
            }
        }

    }
}
