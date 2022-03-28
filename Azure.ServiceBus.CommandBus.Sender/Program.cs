using Azure.ServiceBus.CommandBus;
using Azure.ServiceBus.CommandBus.Sender;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

var queueName = "development";

var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables()
                        .Build();

var serviceProvider = new ServiceCollection()
                        .AddCommandBusSender(options =>
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

logger.LogInformation("Starting Sender...");

await using (var commandBusSender = serviceProvider.GetRequiredService<ICommandBusSender>())
{
    await commandBusSender.SendAsync($"Hello warmup!", queueName);
    var errorResponse = await commandBusSender.SendAsync("Error", queueName);
    if (!errorResponse.IsSuccessStatusCode())
    {
        logger.LogError(errorResponse.ToString());
    }

    var stopWatch = new Stopwatch();
    stopWatch.Start();

    int count = 200;
    // MaxConcurrentCalls must be increased on the Processor if concurrency is increased
    int concurrency = 5;

    var iterator = new AsyncIterator(concurrency);  

    var tasks = Enumerable.Range(0, count).Select(i => i).ToList();

    await iterator.IterateAsync(tasks, default(CancellationToken), async (i) =>
    {
        var response = await commandBusSender.SendAsync($"Hello {i}!", queueName);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError("Error processing command: {error}", ex.Message);
        }
    });

    stopWatch.Stop();
    var elapsedMs = stopWatch.ElapsedMilliseconds;
    logger.LogInformation($"Finished in {elapsedMs}ms");
    var latencyMs = elapsedMs / count;
    logger.LogInformation($"Processed {count} messages with latency: {latencyMs*concurrency}ms, throughput: {(count*1000)/elapsedMs}req/s");
}

logger.LogInformation("Complete!");