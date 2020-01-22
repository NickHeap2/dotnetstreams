using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotnetStreams.Adapters.Kafka
{
    public class KafkaAdminClient : IKafkaAdminClient
    {
        private readonly ILogger<KafkaAdminClient> _logger;
        private readonly IAdminClient _adminClient;

        public KafkaAdminClient(IOptions<AdminClientConfig> adminclientConfig, ILogger<KafkaAdminClient> logger)
        {
            _logger = logger;

            _logger.LogInformation("Building admin client...");
            _adminClient = new AdminClientBuilder(adminclientConfig.Value)
                .SetLogHandler(OnAdminLog)
                .SetErrorHandler(OnAdminError)
                .Build();
        }


        public async Task<bool> EnsureTopicExists(string topicName)
        {
            _logger.LogInformation("Ensuring topic exists...");

            var topicSpecification = new TopicSpecification()
            {
                Name = topicName,
                NumPartitions = 1,
                ReplicationFactor = 1
            };
            var topicSpecifications = new List<TopicSpecification>();
            topicSpecifications.Add(topicSpecification);

            // try and get metadata about topic to check that it exists
            var metaData = _adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(10));
            if (!metaData.Topics[0].Error.IsError)
            {
                _logger.LogInformation("    Topic already exists.");
                return true;
            }

            _logger.LogInformation("    Creating topic...");
            try
            {
                await _adminClient.CreateTopicsAsync(topicSpecifications);
                _logger.LogInformation("        Topic created.");
            }
            catch (CreateTopicsException e)
            {
                // only trap fatal errors so that we let topic already exists error through
                if (e.Error.IsFatal)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnAdminError(IAdminClient adminClient, Error error)
        {
            _logger.LogError(error.Reason);
        }

        private void OnAdminLog(IAdminClient adminClient, LogMessage logMessage)
        {
            if (logMessage.Level == SyslogLevel.Warning)
            {
                _logger.LogWarning(logMessage.Message);
            }
            else if (logMessage.Level == SyslogLevel.Error)
            {
                _logger.LogError(logMessage.Message);
            }
            else
            {
                _logger.LogInformation(logMessage.Message);
            }
        }
    }
}
