namespace VideoCapture.Video.DirectShow
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public static class FilterCategory
    {
        public static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0, 160, 0xc9, 0x11, 0xce, 0x86);
        public static readonly Guid AudioInputDevice = new Guid(0x33d9a762, 0x90c8, 0x11d0, 0xbd, 0x43, 0, 160, 0xc9, 0x11, 0xce, 0x86);
        public static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0, 160, 0xc9, 0x11, 0xce, 0x86);
        public static readonly Guid VideoInputDevice = new Guid(0x860bb310, 0x5d01, 0x11d0, 0xbd, 0x3b, 0, 160, 0xc9, 0x11, 0xce, 0x86);
    }
}

