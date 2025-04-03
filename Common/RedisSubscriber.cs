public class RedisSubscriber : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = RedisHelper.Connection;
        var sub = redis.GetSubscriber();

        await sub.SubscribeAsync("my_channel", (channel, message) =>
        {
            Console.WriteLine($"Received: {message}");
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
