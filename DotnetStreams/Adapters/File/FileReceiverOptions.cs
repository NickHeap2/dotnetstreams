using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetStreams.Adapters.File
{
    public class FileReceiverOptions
    {
        public string TopicName { get; set; }
        public int ReceiveTimeout { get; set; }

        public string TempDirectory { get; set; }

        public string ReceiveDirectory { get; set; }
    }
}
