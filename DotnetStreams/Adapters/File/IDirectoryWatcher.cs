using System;
using System.IO;

namespace DotnetStreams.Adapters.File
{
    public interface IDirectoryWatcher
    {
        event EventHandler<FileSystemEventArgs> Created;

        void StartWatching();
        void StopWatching();
    }
}