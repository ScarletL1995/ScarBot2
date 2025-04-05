using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace ScarBot2.Music
{
    public unsafe class RESAMPLER : IDisposable
    {
        private SwrContext* swr;
        private byte** buffer;
        private int linesize;

        public RESAMPLER(AVCodecContext* context)
        {
            swr = ffmpeg.swr_alloc_set_opts(null,
                (long)AUDIO.ChannelLayout, AUDIO.Format, AUDIO.SampleRate,
                (long)context->channel_layout, context->sample_fmt, context->sample_rate,
                0, null);

            if (swr == null || ffmpeg.swr_init(swr) < 0)
                throw new Exception("Resampler init failed");

            ffmpeg.av_samples_alloc_array_and_samples(&buffer, &linesize, 2, 4096, AUDIO.Format, 0);
        }

        public byte[] Convert(AVFrame* frame)
        {
            int samples = ffmpeg.swr_convert(swr, buffer, 4096, frame->extended_data, frame->nb_samples);
            if (samples <= 0) return Array.Empty<byte>();

            int size = ffmpeg.av_samples_get_buffer_size(&linesize, 2, samples, AUDIO.Format, 1);
            byte[] managed = new byte[size];
            Marshal.Copy((IntPtr)buffer[0], managed, 0, size);

            return managed;
        }

        public void Dispose()
        {
            if (buffer != null)
            {
                ffmpeg.av_freep(&buffer[0]);
                ffmpeg.av_freep(&buffer);
            }
            if (swr != null)
                ffmpeg.swr_free(&swr);
        }
    }
}
