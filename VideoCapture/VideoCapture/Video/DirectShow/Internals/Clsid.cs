namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class Clsid
    {
        public static readonly Guid AsyncReader = new Guid(0xe436ebb5, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        public static readonly Guid CaptureGraphBuilder2 = new Guid(0xbf87b6e1, 0x8c27, 0x11d0, 0xb3, 240, 0, 170, 0, 0x37, 0x61, 0xc5);
        public static readonly Guid FilterGraph = new Guid(0xe436ebb3, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        public static readonly Guid SampleGrabber = new Guid(0xc1f400a0, 0x3f08, 0x11d3, 0x9f, 11, 0, 0x60, 8, 3, 0x9e, 0x37);
        public static readonly Guid SystemDeviceEnum = new Guid(0x62be5d10, 0x60eb, 0x11d0, 0xbd, 0x3b, 0, 160, 0xc9, 0x11, 0xce, 0x86);
    }
}

