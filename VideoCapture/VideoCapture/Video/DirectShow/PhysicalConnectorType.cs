namespace VideoCapture.Video.DirectShow
{
    using System;

    public enum PhysicalConnectorType
    {
        Audio1394 = 0x1007,
        AudioAESDigital = 0x1003,
        AudioAUX = 0x1006,
        AudioDecoder = 0x1009,
        AudioLine = 0x1001,
        AudioMic = 0x1002,
        AudioSCSI = 0x1005,
        AudioSPDIFDigital = 0x1004,
        AudioTuner = 0x1000,
        AudioUSB = 0x1008,
        Default = 0,
        Video1394 = 10,
        VideoAUX = 9,
        VideoBlack = 15,
        VideoComposite = 2,
        VideoDecoder = 12,
        VideoEncoder = 13,
        VideoParallelDigital = 7,
        VideoRGB = 4,
        VideoSCART = 14,
        VideoSCSI = 8,
        VideoSerialDigital = 6,
        VideoSVideo = 3,
        VideoTuner = 1,
        VideoUSB = 11,
        VideoYRYBY = 5
    }
}

