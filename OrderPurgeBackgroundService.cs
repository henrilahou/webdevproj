using project.Data;

public class OrderPurgeBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderPurgeBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentTime = DateTime.Now;
            // Check if current time is 14:30
            if (currentTime.Hour == 14 && currentTime.Minute == 30)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Purge the orders
                var orders = dbContext.Orders.Where(o => o.OrderPlaced < DateTime.UtcNow);
                dbContext.Orders.RemoveRange(orders);
                await dbContext.SaveChangesAsync(stoppingToken);
            }

            // Wait for a minute before checking again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
