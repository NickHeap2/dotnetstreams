using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotnetStreams.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FileFromKafka
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
