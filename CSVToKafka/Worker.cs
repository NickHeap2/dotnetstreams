using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetStreams.Adapters.File;
using DotnetStreams.Adapters.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSVToKafka
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageSender<string, string> _messageSender;
        private readonly IDirectoryWatcher _directoryWatcher;
        ICSVLoader _csvLoader;

        public Worker(IDirectoryWatcher directoryWatcher, ICSVLoader csvLoader, IMessageSender<string, string> messageSender, ILogger<Worker> logger)
        {
            _logger = logger;
            _messageSender = messageSender;
            _directoryWatcher = directoryWatcher;
            _csvLoader = csvLoader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Watching for changes...");
            _directoryWatcher.Created += _directoryWatcher_Created;
            _directoryWatcher.StartWatching();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("FileToKafka worker alive.");
                await Task.Delay(5000, stoppingToken);
            }
        }

        private void _directoryWatcher_Created(object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileName(e.FullPath);
            using (var csvReader = _csvLoader.LoadCSV(e.FullPath))
            {
                var records = csvReader.GetRecords<RecordFormat>();
                foreach (RecordFormat record in records)
                {
                    var key = $"{fileName}_{csvReader.Context.Row}";
                    var value = JsonSerializer.Serialize<RecordFormat>(record);
                    _messageSender.SendMessage(key, value);
                }
            }
        }

    }
}
