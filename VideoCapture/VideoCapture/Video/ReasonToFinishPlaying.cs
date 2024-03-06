namespace VideoCapture.Video
{
    using System;

    public enum ReasonToFinishPlaying
    {
        EndOfStreamReached,
        StoppedByUser,
        DeviceLost,
        VideoSourceError
    }
}

