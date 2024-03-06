namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false), Flags]
    internal enum VideoControlFlags
    {
        ExternalTriggerEnable = 4,
        FlipHorizontal = 1,
        FlipVertical = 2,
        Trigger = 8
    }
}

