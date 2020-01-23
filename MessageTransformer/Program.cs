using DotnetStreams.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace MessageTransformer
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
