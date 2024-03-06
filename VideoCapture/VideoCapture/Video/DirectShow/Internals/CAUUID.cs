namespace VideoCapture.Video.DirectShow.Internals
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    internal struct CAUUID
    {
        public int cElems;
        public IntPtr pElems;
        public Guid[] ToGuidArray()
        {
            Guid[] guidArray = new Guid[this.cElems];
            for (int i = 0; i < this.cElems; i++)
            {
                IntPtr ptr = new IntPtr(this.pElems.ToInt64() + (i * Marshal.SizeOf(typeof(Guid))));
                guidArray[i] = (Guid) Marshal.PtrToStructure(ptr, typeof(Guid));
            }
            return guidArray;
        }
    }
}

