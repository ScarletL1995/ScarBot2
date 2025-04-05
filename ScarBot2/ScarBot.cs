using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LoggingLib;
using MYSQL;
using System.Reflection;

public class SCARBOT
{
    private readonly string token = "n";
    private readonly DiscordSocketClient client;
    private readonly InteractionService interactionService;
    private readonly CommandService commandService;
    private readonly IServiceProvider? services = null;
    private string password = "";

    public static LOGGER Log = new LOGGER("ScarBot.log");
    public static MySQL mysql = new MySQL("SCARBOT-SCARBOT.j.aivencloud.com", "avnadmin", password, "ScarBot", 25050);
    public static readonly ulong[] BotOwnerIds = new ulong[] { 924499450922156032, 249755448050188289 };
    public static List<Func<Task>> InitTasks { get; } = new();

    public SCARBOT()
    {
        client = new DiscordSocketClient();
        interactionService = new InteractionService(client);
        commandService = new CommandService();
    }

    public async Task InitAsync()
    {
        foreach (Func<Task> task in SCARBOT.InitTasks)
            await task();

        client.Log += LogAsync;
        client.Ready += ReadyAsync;
        client.SlashCommandExecuted += SlashCommandExecutedAsync;// g
    }

    public async Task StartAsync()
    {
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await client.SetStatusAsync(UserStatus.Online);
        await client.SetGameAsync("Playing Music");
    }

    private Task LogAsync(LogMessage logMessage)
    {
        SCARBOT.Log.Log(logMessage.ToString(), LOGLEVEL.INFO);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        await RegisterSlashCommandsAsync();
    }

    private async Task RegisterSlashCommandsAsync()
    {
        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
    {
        await command.RespondAsync($"Executed {command.Data.Name} command!");
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

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(_client, message);

        if (message.Author.IsBot) return;

        int argPos = 0;
        if (message.HasStringPrefix("!", ref argPos))  // Change your command prefix here
        {
            await _commands.ExecuteAsync(context, argPos, _services);
        }
    }
}
