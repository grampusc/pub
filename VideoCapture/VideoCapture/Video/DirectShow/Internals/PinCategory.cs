namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class PinCategory
    {
        public static readonly Guid Capture = new Guid(0xfb6c4281, 0x353, 0x11d1, 0x90, 0x5f, 0, 0, 0xc0, 0xcc, 0x16, 0xba);
        public static readonly Guid StillImage = new Guid(0xfb6c428a, 0x353, 0x11d1, 0x90, 0x5f, 0, 0, 0xc0, 0xcc, 0x16, 0xba);
    }
}

