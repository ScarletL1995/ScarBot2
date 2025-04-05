using LoggingLib;
using Microsoft.Extensions.DependencyInjection;
using MYSQL;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<CommandHandler>();

        var provider = services.BuildServiceProvider();
        var bot = provider.GetRequiredService<SCARBOT>();

        await bot.StartAsync();
    }
}
