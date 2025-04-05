using System;
using System.IO;
using FFmpeg.AutoGen;

namespace ScarBot2.Music
{
    public unsafe static class FFmpegSetup
    {
        public static void Initialize()
        {
            string root = AppContext.BaseDirectory;
            string libPath = Path.Combine(root, "FFmpeg");

            ffmpeg.RootPath = libPath;
            ffmpeg.avformat_network_init();
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);
        }
    }
}
