using Microsoft.Extensions.DependencyInjection;
using ScarBot.Commands;
using ScarBot.Music;

public class Program
{
    public static void Main(string[] args)
    {
        // Initialize the bot here
        var bot = new SCARBOT();

        // Register services (dependency injection)
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MusicService>() // Register MusicService as singleton
            .AddSingleton<MusicCommands>() // Register MusicCommands
            .BuildServiceProvider();

//        // Pass service collection to SCARBOT for dependency injection
//        bot.Services = serviceCollection;

        // Run bot
//        bot.RunAsync().GetAwaiter().GetResult();
    }
}
