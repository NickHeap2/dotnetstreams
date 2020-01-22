using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Kafka
{
    public interface IKafkaAdminClient
    {
        Task<bool> EnsureTopicExists(string topicName);
    }
}