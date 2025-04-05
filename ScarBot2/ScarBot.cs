using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LoggingLib;
using MYSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class ScarBot
{
    private readonly string token = "YOUR_BOT_TOKEN_HERE";
    private readonly DiscordSocketClient client;
    private readonly InteractionService interactionService;
    private readonly CommandService commandService;
    private readonly IServiceProvider? services = null;
    private static string password = "YOUR_MYSQL_PASSWORD_HERE";
    public static LOGGER Log = new LOGGER("ScarBot.log");
    public static MySQL mysql = new MySQL("SCARBOT-SCARBOT.j.aivencloud.com", "avnadmin", password, "ScarBot", 25050);
    public static readonly ulong[] BotOwnerIds = new ulong[] { 924499450922156032, 249755448050188289 };
    public static List<Func<Task>> InitTasks { get; } = new();

    public static async Task Main(string[] args)
    {
        ScarBot bot = new ScarBot();
        await bot.InitAsync();
        await bot.StartAsync();
        await Task.Delay(-1);
    }

    public ScarBot()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        client = new DiscordSocketClient(config);
        interactionService = new InteractionService(client);
        commandService = new CommandService();

        RegisterInitTask(async () =>
        {
            string query = "CREATE TABLE IF NOT EXISTS settings (guild_id BIGINT PRIMARY KEY NOT NULL)";
            if (mysql.Execute(query))
                Log.Log("Table 'settings' verified or created", LOGLEVEL.INFO);
            else
                Log.Log("Failed to verify/create table 'settings'", LOGLEVEL.ERROR);
        });
    }

    public async Task InitAsync()
    {
        foreach (Func<Task> task in InitTasks)
            await task();

        client.Log += LogAsync;
        client.Ready += ReadyAsync;
        client.InteractionCreated += HandleInteractionAsync;
        client.MessageReceived += HandleCommandAsync;

        await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        Log.Log("Initialization completed", LOGLEVEL.INFO);
    }

    public async Task StartAsync()
    {
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await client.SetStatusAsync(UserStatus.Online);
        await client.SetGameAsync("Playing Music");

        Log.Log("Bot started successfully", LOGLEVEL.INFO);
    }

    private Task LogAsync(LogMessage logMessage)
    {
        Log.Log(logMessage.ToString(), LOGLEVEL.INFO);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        await RegisterSlashCommandsAsync();
        Log.Log("Bot is ready and slash commands are registered", LOGLEVEL.INFO);
    }

    private async Task RegisterSlashCommandsAsync()
    {
        await interactionService.RegisterCommandsGloballyAsync();
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(client, interaction);
        await interactionService.ExecuteCommandAsync(context, services);
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || message.Author.IsBot)
            return;

        int argPos = 0;
        if (userMessage.HasStringPrefix("!", ref argPos))
        {
            var context = new SocketCommandContext(client, userMessage);
            await commandService.ExecuteAsync(context, argPos, services);
        }
    }

    public static bool IsBotOwner(ulong userId)
    {
        return BotOwnerIds.Contains(userId);
    }

    public static void RegisterInitTask(Func<Task> task)
    {
        InitTasks.Add(task);
    }
}
