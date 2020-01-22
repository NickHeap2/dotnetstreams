namespace DotnetStreams.Adapters.File
{
    public interface IFileSender
    {
        void SendFile(string fileName, string fullPath);
    }
}