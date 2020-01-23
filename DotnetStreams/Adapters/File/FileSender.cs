using DotnetStreams.Adapters.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotnetStreams.Adapters.File
{
    public class FileSender : IFileSender
    {
        private readonly ILogger<FileSender> _logger;
        private readonly IKafkaProducer<string, byte[]> _kafkaProducer;
        private readonly FileSenderOptions _fileSenderOptions;

        public FileSender(IOptions<FileSenderOptions> fileSenderOptions, IKafkaProducer<string, byte[]> kafkaProducer, ILogger<FileSender> logger)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _fileSenderOptions = fileSenderOptions.Value;
        }

        public async void SendFile(string fileName, string fullPath)
        {
            int chunkSize = (_fileSenderOptions.ChunkSizeBytes > 0) ? _fileSenderOptions.ChunkSizeBytes : 102400;
            int headerSize = (sizeof(long) * 3);
            var fileInfo = new FileInfo(fullPath);
            var numberOfChunks = (fileInfo.Length / chunkSize) + 1;

            long chunkNumber = 0;
            using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, chunkSize))
            {
                // read file
                while (fs.Position < fs.Length)
                {
                    // get a correctly sized buffer
                    long remainingBytes = fs.Length - fs.Position;
                    int bufferSize = (int)Math.Min(chunkSize, remainingBytes) + headerSize;
                    byte[] fileChunk = new byte[bufferSize];

                    // get a chunk of data
                    chunkNumber++;
                    Buffer.BlockCopy(BitConverter.GetBytes(fileInfo.Length), 0, fileChunk, 0, sizeof(long));
                    Buffer.BlockCopy(BitConverter.GetBytes(chunkNumber), 0, fileChunk, sizeof(long), sizeof(long));
                    Buffer.BlockCopy(BitConverter.GetBytes(numberOfChunks), 0, fileChunk, sizeof(long) * 2, sizeof(long));

                    var bytesRead = await fs.ReadAsync(fileChunk, headerSize, bufferSize - headerSize);

                    //send the chunk
                    try
                    {
                        var sent = await _kafkaProducer.SendToKafka(_fileSenderOptions.TopicName, fileName, fileChunk);
                        if (sent)
                        {
                            _logger.LogInformation("Sent Filename={FileName} Chunk={ChunkNumber} TotalChunks={NumberOfChunks} ok", fileName, chunkNumber, numberOfChunks);
                        }
                        else
                        {
                            _logger.LogError("Failed to send Filename={FileName} Chunk={ChunkNumber} TotalChunks={NumberOfChunks}!", fileName, chunkNumber, numberOfChunks);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception sending Filename={FileName} Chunk={ChunkNumber} TotalChunks={NumberOfChunks}!", fileName, chunkNumber, numberOfChunks);
                    }
                }
            }

        }
    }
}
