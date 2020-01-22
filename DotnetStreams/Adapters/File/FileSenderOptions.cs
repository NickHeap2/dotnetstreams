using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetStreams.Adapters.File
{
    public class FileSenderOptions
    {
        public string TopicName { get; set; }
        public string WatchFolderName { get; set; }
        public string WatchExtension { get; set; }
    }
}
