using CsvHelper;
using CsvHelper.Configuration;

namespace DotnetStreams.Adapters.File
{
    public interface ICSVLoader
    {
        public CsvReader LoadCSV(string fileName, CsvConfiguration csvConfiguration = null)
    }
}