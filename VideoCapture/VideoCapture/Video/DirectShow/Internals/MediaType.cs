namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class MediaType
    {
        public static readonly Guid Audio = new Guid(0x73647561, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
        public static readonly Guid Interleaved = new Guid(0x73766169, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
        public static readonly Guid Stream = new Guid(0xe436eb83, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        public static readonly Guid Text = new Guid(0x73747874, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
        public static readonly Guid Video = new Guid(0x73646976, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
    }
}

