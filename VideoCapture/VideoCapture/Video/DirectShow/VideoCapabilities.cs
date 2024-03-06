namespace VideoCapture.Video.DirectShow
{
    using VideoCapture.Video.DirectShow.Internals;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public class VideoCapabilities
    {
        public readonly int AverageFrameRate;
        public readonly int BitCount;
        public readonly Size FrameSize;
        public readonly int MaximumFrameRate;

        internal VideoCapabilities()
        {
        }

        internal VideoCapabilities(IAMStreamConfig videoStreamConfig, int index)
        {
            AMMediaType mediaType = null;
            VideoStreamConfigCaps streamConfigCaps = new VideoStreamConfigCaps();
            try
            {
                int errorCode = videoStreamConfig.GetStreamCaps(index, out mediaType, streamConfigCaps);
                if (errorCode != 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (mediaType.FormatType == FormatType.VideoInfo)
                {
                    VideoInfoHeader header = (VideoInfoHeader) Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                    this.FrameSize = new Size(header.BmiHeader.Width, header.BmiHeader.Height);
                    this.BitCount = header.BmiHeader.BitCount;
                    this.AverageFrameRate = (int) (0x989680L / header.AverageTimePerFrame);
                    this.MaximumFrameRate = (int) (0x989680L / streamConfigCaps.MinFrameInterval);
                }
                else
                {
                    if (mediaType.FormatType != FormatType.VideoInfo2)
                    {
                        throw new ApplicationException("Unsupported format found.");
                    }
                    VideoInfoHeader2 header2 = (VideoInfoHeader2) Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader2));
                    this.FrameSize = new Size(header2.BmiHeader.Width, header2.BmiHeader.Height);
                    this.BitCount = header2.BmiHeader.BitCount;
                    this.AverageFrameRate = (int) (0x989680L / header2.AverageTimePerFrame);
                    this.MaximumFrameRate = (int) (0x989680L / streamConfigCaps.MinFrameInterval);
                }
                if (this.BitCount <= 12)
                {
                    throw new ApplicationException("Unsupported format found.");
                }
            }
            finally
            {
                if (mediaType != null)
                {
                    mediaType.Dispose();
                }
            }
        }

        public bool Equals(VideoCapabilities vc2)
        {
            if (vc2 == null)
            {
                return false;
            }
            return ((this.FrameSize == vc2.FrameSize) && (this.BitCount == vc2.BitCount));
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as VideoCapabilities);
        }

        internal static VideoCapabilities[] FromStreamConfig(IAMStreamConfig videoStreamConfig)
        {
            int num;
            int num2;
            if (videoStreamConfig == null)
            {
                throw new ArgumentNullException("videoStreamConfig");
            }
            int numberOfCapabilities = videoStreamConfig.GetNumberOfCapabilities(out num, out num2);
            if (numberOfCapabilities != 0)
            {
                Marshal.ThrowExceptionForHR(numberOfCapabilities);
            }
            if (num <= 0)
            {
                throw new NotSupportedException("This video device does not report capabilities.");
            }
            if (num2 > Marshal.SizeOf(typeof(VideoStreamConfigCaps)))
            {
                throw new NotSupportedException("Unable to retrieve video device capabilities. This video device requires a larger VideoStreamConfigCaps structure.");
            }
            Dictionary<uint, VideoCapabilities> dictionary = new Dictionary<uint, VideoCapabilities>();
            for (int i = 0; i < num; i++)
            {
                try
                {
                    VideoCapabilities capabilities = new VideoCapabilities(videoStreamConfig, i);
                    uint key = (uint) (capabilities.FrameSize.Height | (capabilities.FrameSize.Width << 0x10));
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, capabilities);
                    }
                    else if (capabilities.BitCount > dictionary[key].BitCount)
                    {
                        dictionary[key] = capabilities;
                    }
                }
                catch
                {
                }
            }
            VideoCapabilities[] array = new VideoCapabilities[dictionary.Count];
            dictionary.Values.CopyTo(array, 0);
            return array;
        }

        public override int GetHashCode()
        {
            return (this.FrameSize.GetHashCode() ^ this.BitCount);
        }

        public static bool operator ==(VideoCapabilities a, VideoCapabilities b)
        {
            return (object.ReferenceEquals(a, b) || (((a != null) && (b != null)) && a.Equals(b)));
        }

        public static bool operator !=(VideoCapabilities a, VideoCapabilities b)
        {
            return !(a == b);
        }

        [Obsolete("No longer supported. Use AverageFrameRate instead.")]
        public int FrameRate
        {
            get
            {
                return this.AverageFrameRate;
            }
        }
    }
}

