using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace ScarBot.Commands
{
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        private readonly MusicService _musicService; // Declare MusicService

        // Inject MusicService in the constructor
        public MusicCommands(MusicService musicService)
        {
            _musicService = musicService;
        }

        // Example of adding a song to the queue
        [Command("play")]
        public async Task PlayAsync([Remainder] string query)
        {
            ulong guildId = Context.Guild.Id;

            // Check if the music queue exists and has songs
            if (_musicService.MusicQueue.TryGetValue(guildId, out Queue<string>? queue))
            {
                if (queue.Count == 0)
                {
                    await ReplyAsync("The queue is empty.");
                }
                else
                {
                    // Proceed with your play logic, for example:
                    await _musicService.PlayAsync(query, guildId);
                }
            }
            else
            {
                await ReplyAsync("No queue found.");
            }
        }

        // Example of viewing the queue
        [Command("queue")]
        public async Task QueueAsync()
        {
            ulong guildId = Context.Guild.Id;

            if (_musicService.MusicQueue.TryGetValue(guildId, out Queue<string>? queue))
            {
                if (queue.Count == 0)
                {
                    await ReplyAsync("The queue is empty.");
                }
                else
                {
                    var queueList = string.Join("\n", queue.Select((song, index) => $"{index + 1}. {song}"));
                    await ReplyAsync($"**Current Queue:**\n{queueList}");
                }
            }
            else
            {
                await ReplyAsync("No queue found.");
            }
        }

        // Example of skipping a song
        [Command("skip")]
        public async Task SkipAsync()
        {
            ulong guildId = Context.Guild.Id;

            // Skip logic
            await _musicService.SkipAsync(guildId);
            await ReplyAsync("Song skipped.");// g
        }

        // Example of clearing the queue
        [Command("clearqueue")]
        public async Task ClearQueueAsync()
        {
            ulong guildId = Context.Guild.Id;

            _musicService.MusicQueue[guildId]?.Clear();
            await ReplyAsync("Queue cleared.");
        }
    }
}
