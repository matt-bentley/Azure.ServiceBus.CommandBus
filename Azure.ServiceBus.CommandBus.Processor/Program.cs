using CommandBus;
using CommandBus.Application.CommandHandlers;
using CommandBus.Application.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var queueName = "development";

var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables()
                        .Build();

var services = new ServiceCollection()
                        .AddLogging(builder =>
                        {
                            builder.SetMinimumLevel(LogLevel.Information);
                            builder.AddConsole();
                        });

services.AddCommandBus()
        .AddProcessor(options =>
        {
            configuration.GetSection("CommandBus").Bind(options);
        })
        .AddHandlers(typeof(EchoCommandHandler).Assembly);

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"Starting Processor: {queueName}");

await using (var commandBus = serviceProvider.GetRequiredService<ICommandBus>())
{
    commandBus.RegisterHandlers(typeof(EchoCommandHandler).Assembly);

    await commandBus.StartProcessingAsync();

    logger.LogInformation("Processor waiting for messages...");
    Console.ReadKey();
    await commandBus.StopProcessingAsync();
}

logger.LogInformation("Complete!");