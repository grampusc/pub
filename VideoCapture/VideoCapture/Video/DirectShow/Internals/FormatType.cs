namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class FormatType
    {
        public static readonly Guid VideoInfo = new Guid(0x5589f80, 0xc356, 0x11ce, 0xbf, 1, 0, 170, 0, 0x55, 0x59, 90);
        public static readonly Guid VideoInfo2 = new Guid(0xf72a76a0, 0xeb0a, 0x11d0, 0xac, 0xe4, 0, 0, 0xc0, 0xcc, 0x16, 0xba);
    }
}

