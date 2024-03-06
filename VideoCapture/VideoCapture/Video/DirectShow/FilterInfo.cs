namespace VideoCapture.Video.DirectShow
{
    using VideoCapture.Video.DirectShow.Internals;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    public class FilterInfo : IComparable
    {
        [CompilerGenerated]
        private string <MonikerString>k__BackingField;
        [CompilerGenerated]
        private string <Name>k__BackingField;

        internal FilterInfo(IMoniker moniker)
        {
            this.MonikerString = this.GetMonikerString(moniker);
            this.Name = this.GetName(moniker);
        }

        public FilterInfo(string monikerString)
        {
            this.MonikerString = monikerString;
            this.Name = this.GetName(monikerString);
        }

        public int CompareTo(object value)
        {
            Boka.VideoCapture.Video.DirectShow.FilterInfo info = (Boka.VideoCapture.Video.DirectShow.FilterInfo) value;
            if (info == null)
            {
                return 1;
            }
            return this.Name.CompareTo(info.Name);
        }

        public static object CreateFilter(string filterMoniker)
        {
            object ppvResult = null;
            IBindCtx ppbc = null;
            IMoniker ppmk = null;
            int pchEaten = 0;
            if (Win32.CreateBindCtx(0, out ppbc) == 0)
            {
                if (Win32.MkParseDisplayName(ppbc, filterMoniker, ref pchEaten, out ppmk) == 0)
                {
                    Guid gUID = typeof(IBaseFilter).GUID;
                    ppmk.BindToObject(null, null, ref gUID, out ppvResult);
                    Marshal.ReleaseComObject(ppmk);
                }
                Marshal.ReleaseComObject(ppbc);
            }
            return ppvResult;
        }

        private string GetMonikerString(IMoniker moniker)
        {
            string str;
            moniker.GetDisplayName(null, null, out str);
            return str;
        }

        private string GetName(IMoniker moniker)
        {
            object ppvObj = null;
            IPropertyBag bag = null;
            string str;
            try
            {
                Guid gUID = typeof(IPropertyBag).GUID;
                moniker.BindToStorage(null, null, ref gUID, out ppvObj);
                bag = (IPropertyBag) ppvObj;
                object pVar = "";
                int errorCode = bag.Read("FriendlyName", ref pVar, IntPtr.Zero);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                string str2 = (string) pVar;
                if ((str2 == null) || (str2.Length < 1))
                {
                    throw new ApplicationException();
                }
                str = str2;
            }
            catch (Exception)
            {
                str = "";
            }
            finally
            {
                bag = null;
                if (ppvObj != null)
                {
                    Marshal.ReleaseComObject(ppvObj);
                    ppvObj = null;
                }
            }
            return str;
        }

        private string GetName(string monikerString)
        {
            IBindCtx ppbc = null;
            IMoniker ppmk = null;
            string name = "";
            int pchEaten = 0;
            if (Win32.CreateBindCtx(0, out ppbc) == 0)
            {
                if (Win32.MkParseDisplayName(ppbc, monikerString, ref pchEaten, out ppmk) == 0)
                {
                    name = this.GetName(ppmk);
                    Marshal.ReleaseComObject(ppmk);
                    ppmk = null;
                }
                Marshal.ReleaseComObject(ppbc);
                ppbc = null;
            }
            return name;
        }

        public string MonikerString
        {
            [CompilerGenerated]
            get
            {
                return this.<MonikerString>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<MonikerString>k__BackingField = value;
            }
        }

        public string Name
        {
            [CompilerGenerated]
            get
            {
                return this.<Name>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<Name>k__BackingField = value;
            }
        }
    }
}

