using FFmpeg.AutoGen;

namespace ScarBot2.Music
{
    public static class AUDIO
    {
        public const int SampleRate = 48000;
        public const AVSampleFormat Format = AVSampleFormat.AV_SAMPLE_FMT_FLT;
        public const ulong ChannelLayout = (ulong)AVChannelLayout.AV_CH_LAYOUT_STEREO;
    }
}
