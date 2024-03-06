namespace VideoCapture.Video.DirectShow
{
    using VideoCapture.Video.DirectShow.Internals;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    public class FilterInfoCollection : CollectionBase
    {
        public FilterInfoCollection(Guid category)
        {
            this.CollectFilters(category);
        }

        private void CollectFilters(Guid category)
        {
            object o = null;
            ICreateDevEnum enum2 = null;
            IEnumMoniker enumMoniker = null;
            IMoniker[] rgelt = new IMoniker[1];
            try
            {
                bool flag;
                Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
                if (typeFromCLSID == null)
                {
                    throw new ApplicationException("Failed creating device enumerator");
                }
                o = Activator.CreateInstance(typeFromCLSID);
                enum2 = (ICreateDevEnum) o;
                if (enum2.CreateClassEnumerator(ref category, out enumMoniker, 0) != 0)
                {
                    throw new ApplicationException("No devices of the category");
                }
                IntPtr zero = IntPtr.Zero;
                goto Label_00B5;
            Label_006D:
                if ((enumMoniker.Next(1, rgelt, zero) != 0) || (rgelt[0] == null))
                {
                    goto Label_00BA;
                }
                Boka.VideoCapture.Video.DirectShow.FilterInfo info = new Boka.VideoCapture.Video.DirectShow.FilterInfo(rgelt[0]);
                base.InnerList.Add(info);
                Marshal.ReleaseComObject(rgelt[0]);
                rgelt[0] = null;
            Label_00B5:
                flag = true;
                goto Label_006D;
            Label_00BA:
                base.InnerList.Sort();
            }
            catch
            {
            }
            finally
            {
                enum2 = null;
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                    o = null;
                }
                if (enumMoniker != null)
                {
                    Marshal.ReleaseComObject(enumMoniker);
                    enumMoniker = null;
                }
                if (rgelt[0] != null)
                {
                    Marshal.ReleaseComObject(rgelt[0]);
                    rgelt[0] = null;
                }
            }
        }

        public Boka.VideoCapture.Video.DirectShow.FilterInfo this[int index]
        {
            get
            {
                return (Boka.VideoCapture.Video.DirectShow.FilterInfo) base.InnerList[index];
            }
        }
    }
}

