using Discord.WebSocket;
using LoggingLib;
using MYSQL;

public class MusicHandler
{
    private readonly IAudioService _audioService;
    private readonly MySQL _mysql;
    private readonly Dictionary<ulong, MusicQueue> _queues = new Dictionary<ulong, MusicQueue>(); // Queue per server
    private readonly Dictionary<ulong, MusicSettings> _musicSettings = new Dictionary<ulong, MusicSettings>(); // Settings per server

    public MusicHandler(IAudioService audioService, MySQL mysql)
    {
        _audioService = audioService;
        _mysql = mysql;
    }

    public async Task JoinVoiceChannel(SocketVoiceChannel voiceChannel)
    {
        await _audioService.JoinChannel(voiceChannel);
        ScarBot.Log($"Joined channel {voiceChannel.Name}.");
    }

    public async Task LeaveVoiceChannel(SocketVoiceChannel voiceChannel)
    {
        await _audioService.LeaveChannel(voiceChannel);
        ScarBot.Log($"Left channel {voiceChannel.Name}.");
    }

    public async Task PlaySong(ulong serverId, string songUrl)
    {
        var queue = _queues.ContainsKey(serverId) ? _queues[serverId] : new MusicQueue();
        queue.AddSong(songUrl);
        _queues[serverId] = queue;

        var song = await YouTubeExplodeClient.GetSongAsync(songUrl);
        await _audioService.PlaySong(song);

        ScarBot.Log($"Playing song: {song.Title}");
    }

    public async Task PauseSong(ulong serverId)
    {
        await _audioService.Pause();
        ScarBot.Log($"Paused song for server {serverId}");
    }

    public async Task StopSong(ulong serverId)
    {
        await _audioService.Stop();
        ScarBot.Log($"Stopped song for server {serverId}");
    }

    public async Task ResumeSong(ulong serverId)
    {
        await _audioService.Resume();
        ScarBot.Log($"Resumed song for server {serverId}");
    }

    public async Task SkipSong(ulong serverId)
    {
        var queue = _queues.ContainsKey(serverId) ? _queues[serverId] : new MusicQueue();
        queue.SkipSong();
        _queues[serverId] = queue;

        var song = await YouTubeExplodeClient.GetSongAsync(queue.Songs[queue.CurrentSongIndex]);
        await _audioService.PlaySong(song);

        ScarBot.Log($"Skipped to next song for server {serverId}");
    }

    public async Task SetRepeat(ulong serverId, bool repeat)
    {
        if (!_musicSettings.ContainsKey(serverId))
            _musicSettings[serverId] = new MusicSettings();

        _musicSettings[serverId].Repeat = repeat;
        ScarBot.Log($"Set repeat for server {serverId} to {repeat}");
    }

    public async Task Set247(ulong serverId, bool is247)
    {
        if (!_musicSettings.ContainsKey(serverId))
            _musicSettings[serverId] = new MusicSettings();

        _musicSettings[serverId].Is247 = is247;
        ScarBot.Log($"Set 247 for server {serverId} to {is247}");
    }
}

public class MusicQueue
{
    public List<string> Songs { get; private set; } = new List<string>();
    public int CurrentSongIndex { get; set; } = 0;

    public void AddSong(string songUrl)
    {
        Songs.Add(songUrl);
    }

    public void SkipSong()
    {
        if (CurrentSongIndex < Songs.Count - 1)
            CurrentSongIndex++;
    }
}

public class MusicSettings
{
    public bool Repeat { get; set; } = false;
    public bool Is247 { get; set; } = false;
}
