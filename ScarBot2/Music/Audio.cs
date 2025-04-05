using FFmpeg.AutoGen;

public unsafe class AUDIO
{
    private AVFormatContext* pFormatContext;
    private AVCodecContext* pCodecContext;
    private AVCodec* pCodec;
    private int audioStreamIndex;

    public AUDIO(string filePath)
    {
        FFmpegBinariesHelper.RegisterFFmpegBinaries();

        pFormatContext = ffmpeg.avformat_alloc_context();
        if (ffmpeg.avformat_open_input(&pFormatContext, filePath, null, null) != 0)
        {
            throw new Exception("Could not open file.");
        }

        if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
        {
            throw new Exception("Could not find stream information.");
        }

        audioStreamIndex = -1;
        for (int i = 0; i < pFormatContext->nb_streams; i++)
        {
            if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                audioStreamIndex = i;
                break;
            }
        }

        if (audioStreamIndex == -1)
        {
            throw new Exception("Could not find audio stream.");
        }

        pCodecContext = ffmpeg.avcodec_alloc_context3(null);
        ffmpeg.avcodec_parameters_to_context(pCodecContext, pFormatContext->streams[audioStreamIndex]->codecpar);

        pCodec = ffmpeg.avcodec_find_decoder(pCodecContext->codec_id);
        if (pCodec == null)
        {
            throw new Exception("Codec not found.");
        }

        if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
        {
            throw new Exception("Could not open codec.");
        }
    }

    public void PlayAudio()
    {
        AVPacket packet;
        AVFrame* pFrame = ffmpeg.av_frame_alloc();
        while (ffmpeg.av_read_frame(pFormatContext, &packet) >= 0)
        {
            if (packet.stream_index == audioStreamIndex)
            {
                int response = ffmpeg.avcodec_send_packet(pCodecContext, &packet);
                if (response < 0)
                {
                    throw new Exception("Error while sending packet to codec.");
                }

                response = ffmpeg.avcodec_receive_frame(pCodecContext, pFrame);
                if (response >= 0)
                {
                    // Process audio frame here
                }
            }
            ffmpeg.av_packet_unref(&packet);
        }
    }
}
