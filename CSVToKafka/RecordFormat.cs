using CsvHelper.Configuration.Attributes;

namespace CSVToKafka
{
    public class RecordFormat
    {
        [Index(0)]
        public int LineNumber { get; set; }
        [Index(1)]
        public string OrderNumber { get; set; }
        [Index(2)]
        public int OrderLineCount { get; set; }
    }
}