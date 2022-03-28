using Azure.ServiceBus.CommandBus;
using Azure.ServiceBus.CommandBus.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var queueName = "development";

var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables()
                        .Build();

var serviceProvider = new ServiceCollection()
                        .AddCommandBusProcessor(options =>
                        {
                            configuration.GetSection("CommandBus").Bind(options);
                        })
                        .AddLogging(builder =>
                        {
                            builder.SetMinimumLevel(LogLevel.Information);
                            builder.AddConsole();
                        })
                        .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"Starting Processor: {queueName}");

await using (var commandBusProcessor = serviceProvider.GetRequiredService<ICommandBusProcessor>())
{
    commandBusProcessor.RegisterHandler(async (message) =>
    {
        logger.LogInformation("Processing message: {message}", message);
        if (message == "Error")
        {
            throw new ApplicationException("There was an error");
        }
        if (message == "Hello 10!")
        {
            throw new CommandValidationException($"Invalid message: {message}");
        }
        await Task.CompletedTask;
    });

    await commandBusProcessor.StartProcessingAsync();

    logger.LogInformation("Processor waiting for messages...");
    Console.ReadKey();
    await commandBusProcessor.StopProcessingAsync();
}

logger.LogInformation("Complete!");