namespace DotnetStreams.Adapters.File
{
    public interface IFileReceiver
    {
        bool ReceiveFileChunk();
    }
}