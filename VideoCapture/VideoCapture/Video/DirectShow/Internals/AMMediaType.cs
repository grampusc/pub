namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    internal class AMMediaType : IDisposable
    {
        public Guid MajorType;
        public Guid SubType;
        [MarshalAs(UnmanagedType.Bool)]
        public bool FixedSizeSamples = true;
        [MarshalAs(UnmanagedType.Bool)]
        public bool TemporalCompression;
        public int SampleSize = 1;
        public Guid FormatType;
        public IntPtr unkPtr;
        public int FormatSize;
        public IntPtr FormatPtr;
        ~AMMediaType()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((this.FormatSize != 0) && (this.FormatPtr != IntPtr.Zero))
            {
                Marshal.FreeCoTaskMem(this.FormatPtr);
                this.FormatSize = 0;
            }
            if (this.unkPtr != IntPtr.Zero)
            {
                Marshal.Release(this.unkPtr);
                this.unkPtr = IntPtr.Zero;
            }
        }
    }
}

