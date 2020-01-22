using DotnetStreams.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DotnetStreams.Adapters.File
{
    public class DirectoryWatcher : IDirectoryWatcher
    {
        private readonly ILogger<DirectoryWatcher> _logger;
        private readonly DirectoryWatcherOptions _directoryWatcherOptions;

        private readonly FileSystemWatcher _fileSystemWatcher;

        public event EventHandler<FileSystemEventArgs> Created;

        public DirectoryWatcher(IOptions<DirectoryWatcherOptions> directoryWatcherOptions, ILogger<DirectoryWatcher> logger)
        {
            _logger = logger;
            _directoryWatcherOptions = directoryWatcherOptions.Value;

            _fileSystemWatcher = new FileSystemWatcher(_directoryWatcherOptions.Directory, _directoryWatcherOptions.FileFilter);
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Processing {e.FullPath}...");

            FileHelper.WaitForFileToUnlock(e.FullPath);

            Created?.Invoke(this, e);
        }

        public void StartWatching()
        {
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
        }

    }
}
