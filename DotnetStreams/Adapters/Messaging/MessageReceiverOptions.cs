using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetStreams.Adapters.Messaging
{
    public class MessageReceiverOptions
    {
        public string TopicName { get; set; }
        public int ReceiveTimeout { get; set; }
    }
}
