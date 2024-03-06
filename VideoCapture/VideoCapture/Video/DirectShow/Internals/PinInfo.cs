namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=1), ComVisible(false)]
    internal struct PinInfo
    {
        public IBaseFilter Filter;
        public PinDirection Direction;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
        public string Name;
    }
}

