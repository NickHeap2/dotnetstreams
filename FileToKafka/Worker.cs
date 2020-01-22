using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotnetStreams.Adapters.File;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileToKafka
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFileSender _fileSender;
        private readonly IDirectoryWatcher _directoryWatcher;

        public Worker(IDirectoryWatcher directoryWatcher, IFileSender fileSender, ILogger<Worker> logger)
        {
            _logger = logger;
            _fileSender = fileSender;
            _directoryWatcher = directoryWatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watching for changes...");
            _directoryWatcher.Created += _directoryWatcher_Created;
            _directoryWatcher.StartWatching();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void _directoryWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _fileSender.SendFile(e.Name, e.FullPath);
        }

    }
}
