using Microsoft.AspNetCore.SignalR;

public class RedisSubscriber : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RedisSubscriber(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var redis = RedisHelper.Connection;
            var sub = redis.GetSubscriber();

            await sub.SubscribeAsync("my_channel", async (channel, message) =>
            {
                Console.WriteLine($"Received from Redis: {message}");

                // Send message to all connected SignalR clients
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.ToString());
            });

            // Keep the background service alive
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RedisSubscriber Exception: {ex.Message} \n{ex.StackTrace}");
        }
    }
}
