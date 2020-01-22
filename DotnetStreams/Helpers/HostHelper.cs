using Confluent.Kafka;
using DotnetStreams.Adapters.File;
using DotnetStreams.Adapters.Kafka;
using DotnetStreams.Adapters.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DotnetStreams.Helpers
{
    public class HostHelper
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    // get host config from hostsettings.json and env vars
                    configHost.SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("hostsettings.json", optional: true)
                              .AddEnvironmentVariables()
                              .AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    // get app config from appsettings.json, appsetttings.{ENV}.json and env vars
                    configApp.AddJsonFile("appsettings.json", optional: true)
                             .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                             .AddEnvironmentVariables()
                             .AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // create serilogger from configuration and hook a close and flush into process exit
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .CreateLogger();
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

                    // kafka
                    string clientConfig = "Adapters:Kafka:Client";
                    string producerConfig = "Adapters:Kafka:Producer";
                    if (hostContext.Configuration.GetSection(producerConfig).Exists())
                    {
                        services.Configure<ProducerConfig>(options =>
                        {
                            hostContext.Configuration.GetSection(clientConfig).Bind(options);
                            hostContext.Configuration.GetSection(producerConfig).Bind(options);
                        })
                                .AddSingleton<IKafkaProducer<string, byte[]>, KafkaProducer<string, byte[]>>()
                                .AddSingleton<IKafkaProducer<string, string>, KafkaProducer<string, string>>();
                    }
                    string consumerConfig = "Adapters:Kafka:Consumer";
                    if (hostContext.Configuration.GetSection(consumerConfig).Exists())
                    {
                        services.Configure<ConsumerConfig>(options =>
                        {
                            hostContext.Configuration.GetSection(clientConfig).Bind(options);
                            hostContext.Configuration.GetSection(consumerConfig).Bind(options);
                        })
                                .AddSingleton<IKafkaConsumer<string, byte[]>, KafkaConsumer<string, byte[]>>()
                                .AddSingleton<IKafkaConsumer<string, string>, KafkaConsumer<string, string>>();
                    }
                    string adminClientConfig = "Adapters:Kafka:AdminClient";
                    if (hostContext.Configuration.GetSection(adminClientConfig).Exists()
                        || hostContext.Configuration.GetSection(consumerConfig).Exists()
                        || hostContext.Configuration.GetSection(producerConfig).Exists())
                    {
                        services.Configure<AdminClientConfig>(options =>
                        {
                            hostContext.Configuration.GetSection(clientConfig).Bind(options);
                            hostContext.Configuration.GetSection(adminClientConfig).Bind(options);
                        })
                                .AddSingleton<IKafkaAdminClient, KafkaAdminClient>();
                    }

                    // file
                    string fileSenderConfig = "Adapters:File:FileSender";
                    if (hostContext.Configuration.GetSection(fileSenderConfig).Exists())
                    {
                        services.Configure<FileSenderOptions>(options => hostContext.Configuration.GetSection(fileSenderConfig).Bind(options))
                                .AddSingleton<IFileSender, FileSender>();
                    }
                    string fileReceiverConfig = "Adapters:File:FileReceiver";
                    if (hostContext.Configuration.GetSection(fileReceiverConfig).Exists())
                    {
                        services.Configure<FileReceiverOptions>(options => hostContext.Configuration.GetSection(fileReceiverConfig).Bind(options))
                                .AddSingleton<IFileReceiver, FileReceiver>();
                    }
                    string directoryWatcherConfig = "Adapters:File:DirectoryWatcher";
                    if (hostContext.Configuration.GetSection(directoryWatcherConfig).Exists())
                    {
                        services.Configure<DirectoryWatcherOptions>(options => hostContext.Configuration.GetSection(directoryWatcherConfig).Bind(options))
                                .AddSingleton<IDirectoryWatcher, DirectoryWatcher>();
                    }

                    //message
                    string messageReceiverConfig = "Adapters:Messaging:MessageReceiver";
                    if (hostContext.Configuration.GetSection(messageReceiverConfig).Exists())
                    {
                        services.Configure<MessageReceiverOptions>(options => hostContext.Configuration.GetSection(messageReceiverConfig).Bind(options))
                                .AddSingleton<IMessageReceiver<string, byte[]>, MessageReceiver<string, byte[]>>();
                    }
                    string messageSenderConfig = "Adapters:Messaging:MessageSender";
                    if (hostContext.Configuration.GetSection(messageSenderConfig).Exists())
                    {
                        services.Configure<MessageReceiverOptions>(options => hostContext.Configuration.GetSection(messageSenderConfig).Bind(options))
                                .AddSingleton<IMessageReceiver<string, byte[]>, MessageReceiver<string, byte[]>>();
                    }

                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    // enable serilog as our logger
                    configLogging.AddSerilog(dispose: true);
                })
                .UseConsoleLifetime();
    }
}
