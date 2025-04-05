using System;
using FFmpeg.AutoGen;

namespace ScarBot2.Music
{
    public unsafe class DECODER : IDisposable
    {
        private AVFormatContext* format;
        private AVCodecContext* codec;
        private AVStream* stream;
        private AVPacket* packet;
        private AVFrame* frame;
        private int streamIndex;

        public AVCodecContext* Codec => codec;
        public AVFrame* Frame => frame;

        public DECODER(string file)
        {
            FFmpegSetup.Initialize();

            format = ffmpeg.avformat_alloc_context();
            if (ffmpeg.avformat_open_input(&format, file, null, null) != 0)
                throw new Exception("Could not open input");

            if (ffmpeg.avformat_find_stream_info(format, null) != 0)
                throw new Exception("Could not find stream info");

            for (int i = 0; i < format->nb_streams; i++)
            {
                if (format->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    streamIndex = i;
                    stream = format->streams[i];
                    break;
                }
            }

            if (stream == null)
                throw new Exception("No audio stream");

            AVCodec* decoder = ffmpeg.avcodec_find_decoder(stream->codecpar->codec_id);
            if (decoder == null)
                throw new Exception("Decoder not found");

            codec = ffmpeg.avcodec_alloc_context3(decoder);
            ffmpeg.avcodec_parameters_to_context(codec, stream->codecpar);
            if (ffmpeg.avcodec_open2(codec, decoder, null) != 0)
                throw new Exception("Could not open codec");

            packet = ffmpeg.av_packet_alloc();
            frame = ffmpeg.av_frame_alloc();
        }

        public bool Read()
        {
            while (ffmpeg.av_read_frame(format, packet) >= 0)
            {
                if (packet->stream_index != streamIndex)
                {
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                if (ffmpeg.avcodec_send_packet(codec, packet) < 0)
                {
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                ffmpeg.av_packet_unref(packet);

                return ffmpeg.avcodec_receive_frame(codec, frame) == 0;
            }

            return false;
        }

        public void Dispose()
        {
            ffmpeg.av_frame_free(&frame);
            ffmpeg.av_packet_free(&packet);
            ffmpeg.avcodec_free_context(&codec);
            ffmpeg.avformat_close_input(&format);
        }
    }
}
