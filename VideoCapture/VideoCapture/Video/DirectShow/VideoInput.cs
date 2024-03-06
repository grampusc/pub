namespace VideoCapture.Video.DirectShow
{
    using System;

    public class VideoInput
    {
        public readonly int Index;
        public readonly PhysicalConnectorType Type;

        internal VideoInput(int index, PhysicalConnectorType type)
        {
            this.Index = index;
            this.Type = type;
        }

        public static VideoInput Default
        {
            get
            {
                return new VideoInput(-1, PhysicalConnectorType.Default);
            }
        }
    }
}

