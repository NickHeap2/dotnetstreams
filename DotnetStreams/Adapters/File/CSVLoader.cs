using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotnetStreams.Adapters.File
{
    public class CSVLoader : ICSVLoader
    {
        private readonly ILogger<CSVLoader> _logger;
        private readonly CSVLoaderOptions _csvLoaderOptions;

        public CSVLoader(IOptions<CSVLoaderOptions> fileSenderOptions, ILogger<CSVLoader> logger)
        {
            _logger = logger;
            _csvLoaderOptions = fileSenderOptions.Value;
        }

        public CsvReader LoadCSV(string fileName, CsvConfiguration csvConfiguration = null)
        {
            TextReader textReader = new StreamReader(fileName);
            if (csvConfiguration == null)
            {
                csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = _csvLoaderOptions.HasHeaderRecord
                };
            }
            var csvReader = new CsvReader(textReader, csvConfiguration, false);

            return csvReader;
        }

    }
}
