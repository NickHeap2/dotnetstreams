using Confluent.Kafka;
using DotnetStreams.Adapters.Kafka;
using DotnetStreams.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetStreams.Adapters.File
{
    public class FileReceiver : IFileReceiver
    {
        private readonly ILogger<FileSender> _logger;
        private readonly IKafkaConsumer<string, byte[]> _kafkaConsumer;
        private readonly FileReceiverOptions _fileReceiverOptions;
        private readonly TimeSpan _receiveTimeout;

        public FileReceiver(IOptions<FileReceiverOptions> fileReceiverOptions, IKafkaConsumer<string, byte[]> kafkaConsumer, ILogger<FileSender> logger)
        {
            _logger = logger;
            _kafkaConsumer = kafkaConsumer;
            _fileReceiverOptions = fileReceiverOptions.Value;

            _receiveTimeout = TimeSpan.FromMilliseconds(_fileReceiverOptions.ReceiveTimeout);
        }

        public bool ReceiveFileChunk()
        {
            var receivedFileChunk = _kafkaConsumer.ReceiveFromKafka(_fileReceiverOptions.TopicName, _receiveTimeout);
            if (receivedFileChunk != null)
            {
                _logger.LogDebug("Received file chunk Key={MessageKey} Length={MessageLength}", receivedFileChunk.Key, receivedFileChunk.Value.Length);
                ProcessChunk(receivedFileChunk);
                return true;
            }
            return false;
        }

        private void ProcessChunk(ConsumeResult<string, byte[]> receivedFileChunk)
        {
            string filename = receivedFileChunk.Key;
            string tempFilename = Path.Combine(_fileReceiverOptions.TempDirectory, filename);
            byte[] fileBytes = receivedFileChunk.Value;
            int headerSize = sizeof(long) * 3;

            if (fileBytes.Length < 12)
            {
                _logger.LogError("Received invalid file chunk Key={MessageKey} Length={MessageLength}", receivedFileChunk.Key, receivedFileChunk.Value.Length);
                return;
            }

            long fileLength = BitConverter.ToInt64(fileBytes, 0);
            long chunkNumber = BitConverter.ToInt64(fileBytes, sizeof(long));
            long numberOfChunks = BitConverter.ToInt64(fileBytes, sizeof(long) * 2);
            int fileChunkLength = fileBytes.Length - headerSize;

            _logger.LogInformation("Received file chunk Filename={Filename} Length={FileLength} Chunk={ChunkNumber} TotalChunks={NumerOfChunks}", filename, fileLength, chunkNumber, numberOfChunks);

            if (chunkNumber != 1
                && !System.IO.File.Exists(tempFilename))
            {
                _logger.LogError("Chunk greater than 1 and file {Filename} doesn't exist!", filename);
                return;
            }
            else if (chunkNumber == 1
                     && System.IO.File.Exists(tempFilename))
            {
                FileHelper.WaitForFileToUnlock(tempFilename);
                System.IO.File.Delete(tempFilename);
            }

            // we need to check if chunk number > 1 and file doesn't exist then don't continue with file
            using (FileStream fileStream = new FileStream(tempFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                fileStream.Write(fileBytes, headerSize, fileChunkLength);
                fileStream.Flush();
                fileStream.Close();
            }

            // move file on last chunk
            if (chunkNumber == numberOfChunks)
            {
                string receiveFilename = Path.Combine(_fileReceiverOptions.ReceiveDirectory, filename);
                _logger.LogInformation("Moving completed file to {DestinationFile}...", receiveFilename);

                FileHelper.WaitForFileToUnlock(tempFilename);
                
                System.IO.File.Move(tempFilename, receiveFilename, true);
            }
        }
    }
}
