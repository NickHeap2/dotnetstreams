using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DotnetStreams.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CSVToKafka
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var host = HostHelper.CreateHostBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .Build();
            await host.RunAsync();

            return 0;
        }
    }
}
