namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class FindDirection
    {
        public static readonly Guid DownstreamOnly = new Guid(0xac798be1, 0x98e3, 0x11d1, 0xb3, 0xf1, 0, 170, 0, 0x37, 0x61, 0xc5);
        public static readonly Guid UpstreamOnly = new Guid(0xac798be0, 0x98e3, 0x11d1, 0xb3, 0xf1, 0, 170, 0, 0x37, 0x61, 0xc5);
    }
}

