using Discord.Interactions;
using Discord.WebSocket;

public class MusicCommands
{
    private readonly MusicHandler _musicHandler;

    public MusicCommands(MusicHandler musicHandler)
    {
        _musicHandler = musicHandler;
    }

    [SlashCommand("join", "Join a voice channel")]
    public async Task JoinVoiceChannelAsync(SocketVoiceChannel voiceChannel)
    {
        await _musicHandler.JoinVoiceChannel(voiceChannel);
    }

    [SlashCommand("leave", "Leave the current voice channel")]
    public async Task LeaveVoiceChannelAsync(SocketVoiceChannel voiceChannel)
    {
        await _musicHandler.LeaveVoiceChannel(voiceChannel);
    }

    [SlashCommand("play", "Play a song from YouTube")]
    public async Task PlaySongAsync(string songUrl)
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.PlaySong(serverId, songUrl);
    }

    [SlashCommand("stop", "Stop the current song")]
    public async Task StopSongAsync()
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.StopSong(serverId);
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task PauseSongAsync()
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.PauseSong(serverId);
    }

    [SlashCommand("resume", "Resume the paused song")]
    public async Task ResumeSongAsync()
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.ResumeSong(serverId);
    }

    [SlashCommand("skip", "Skip to the next song")]
    public async Task SkipSongAsync()
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.SkipSong(serverId);
    }

    [SlashCommand("repeat", "Set the repeat mode")]
    public async Task SetRepeatAsync(bool repeat)
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.SetRepeat(serverId, repeat);
    }

    [SlashCommand("247", "Set the 247 mode")]
    public async Task Set247Async(bool is247)
    {
        ulong serverId = Context.Guild.Id;
        await _musicHandler.Set247(serverId, is247);
    }
}
