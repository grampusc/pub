namespace VideoCapture.Video
{
    using System;

    public interface IVideoSource
    {
        event NewFrameEventHandler NewFrame;

        event PlayingFinishedEventHandler PlayingFinished;

        event VideoSourceErrorEventHandler VideoSourceError;

        void SignalToStop();
        void Start();
        void Stop();
        void WaitForStop();

        long BytesReceived { get; }

        int FramesReceived { get; }

        bool IsRunning { get; }

        string Source { get; }
    }
}

