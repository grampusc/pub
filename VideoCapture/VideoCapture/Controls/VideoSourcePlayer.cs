namespace VideoCapture.Controls
{
    using VideoCapture.Imaging;
    using VideoCapture.Video;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    public class VideoSourcePlayer : Control
    {
        private bool autosize;
        private Color borderColor = Color.Black;
        private IContainer components;
        private Bitmap convertedFrame;
        private Bitmap currentFrame;
        private bool firstFrameNotProcessed = true;
        private Size frameSize = new Size(320, 240);
        private string lastMessage;
        private bool needSizeUpdate;
        private NewFrameHandler NewFrame;
        private Control parent;
        private PlayingFinishedEventHandler PlayingFinished;
        private volatile bool requestedToStop;
        private object sync = new object();
        private IVideoSource videoSource;

        public event NewFrameHandler NewFrame
        {
            add
            {
                NewFrameHandler handler2;
                NewFrameHandler newFrame = this.NewFrame;
                do
                {
                    handler2 = newFrame;
                    NewFrameHandler handler3 = (NewFrameHandler) Delegate.Combine(handler2, value);
                    newFrame = Interlocked.CompareExchange<NewFrameHandler>(ref this.NewFrame, handler3, handler2);
                }
                while (newFrame != handler2);
            }
            remove
            {
                NewFrameHandler handler2;
                NewFrameHandler newFrame = this.NewFrame;
                do
                {
                    handler2 = newFrame;
                    NewFrameHandler handler3 = (NewFrameHandler) Delegate.Remove(handler2, value);
                    newFrame = Interlocked.CompareExchange<NewFrameHandler>(ref this.NewFrame, handler3, handler2);
                }
                while (newFrame != handler2);
            }
        }

        public event PlayingFinishedEventHandler PlayingFinished
        {
            add
            {
                PlayingFinishedEventHandler handler2;
                PlayingFinishedEventHandler playingFinished = this.PlayingFinished;
                do
                {
                    handler2 = playingFinished;
                    PlayingFinishedEventHandler handler3 = (PlayingFinishedEventHandler) Delegate.Combine(handler2, value);
                    playingFinished = Interlocked.CompareExchange<PlayingFinishedEventHandler>(ref this.PlayingFinished, handler3, handler2);
                }
                while (playingFinished != handler2);
            }
            remove
            {
                PlayingFinishedEventHandler handler2;
                PlayingFinishedEventHandler playingFinished = this.PlayingFinished;
                do
                {
                    handler2 = playingFinished;
                    PlayingFinishedEventHandler handler3 = (PlayingFinishedEventHandler) Delegate.Remove(handler2, value);
                    playingFinished = Interlocked.CompareExchange<PlayingFinishedEventHandler>(ref this.PlayingFinished, handler3, handler2);
                }
                while (playingFinished != handler2);
            }
        }

        public VideoSourcePlayer()
        {
            this.InitializeComponent();
            base.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        private void CheckForCrossThreadAccess()
        {
            if (!base.IsHandleCreated)
            {
                base.CreateControl();
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
            }
            if (base.InvokeRequired)
            {
                throw new InvalidOperationException("Cross thread access to the control is not allowed.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public Bitmap GetCurrentVideoFrame()
        {
            lock (this.sync)
            {
                return ((this.currentFrame == null) ? null : Boka.VideoCapture.Imaging.Image.Clone(this.currentFrame));
            }
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.Paint += new PaintEventHandler(this.VideoSourcePlayer_Paint);
            base.ParentChanged += new EventHandler(this.VideoSourcePlayer_ParentChanged);
            base.ResumeLayout(false);
        }

        private void parent_SizeChanged(object sender, EventArgs e)
        {
            this.UpdatePosition();
        }

        public void SignalToStop()
        {
            this.CheckForCrossThreadAccess();
            this.requestedToStop = true;
            if (this.videoSource != null)
            {
                this.videoSource.SignalToStop();
            }
        }

        public void Start()
        {
            this.CheckForCrossThreadAccess();
            this.requestedToStop = false;
            if (this.videoSource != null)
            {
                this.firstFrameNotProcessed = true;
                this.videoSource.Start();
                base.Invalidate();
            }
        }

        public void Stop()
        {
            this.CheckForCrossThreadAccess();
            this.requestedToStop = true;
            if (this.videoSource != null)
            {
                this.videoSource.Stop();
                if (this.currentFrame != null)
                {
                    this.currentFrame.Dispose();
                    this.currentFrame = null;
                }
                base.Invalidate();
            }
        }

        private void UpdatePosition()
        {
            if ((this.autosize && (this.Dock != DockStyle.Fill)) && (base.Parent != null))
            {
                Rectangle clientRectangle = base.Parent.ClientRectangle;
                int width = this.frameSize.Width;
                int height = this.frameSize.Height;
                base.SuspendLayout();
                base.Location = new Point(((clientRectangle.Width - width) - 2) / 2, ((clientRectangle.Height - height) - 2) / 2);
                base.Size = new Size(width + 2, height + 2);
                base.ResumeLayout();
            }
        }

        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!this.requestedToStop)
            {
                Bitmap image = (Bitmap) eventArgs.Frame.Clone();
                if (this.NewFrame != null)
                {
                    this.NewFrame(this, ref image);
                }
                lock (this.sync)
                {
                    if (this.currentFrame != null)
                    {
                        if (this.currentFrame.Size != eventArgs.Frame.Size)
                        {
                            this.needSizeUpdate = true;
                        }
                        this.currentFrame.Dispose();
                        this.currentFrame = null;
                    }
                    if (this.convertedFrame != null)
                    {
                        this.convertedFrame.Dispose();
                        this.convertedFrame = null;
                    }
                    this.currentFrame = image;
                    this.frameSize = this.currentFrame.Size;
                    this.lastMessage = null;
                    if (((this.currentFrame.PixelFormat == PixelFormat.Format16bppGrayScale) || (this.currentFrame.PixelFormat == PixelFormat.Format48bppRgb)) || (this.currentFrame.PixelFormat == PixelFormat.Format64bppArgb))
                    {
                        this.convertedFrame = Boka.VideoCapture.Imaging.Image.Convert16bppTo8bpp(this.currentFrame);
                    }
                }
                base.Invalidate();
            }
        }

        private void videoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            switch (reason)
            {
                case ReasonToFinishPlaying.EndOfStreamReached:
                    this.lastMessage = "Video has finished";
                    break;

                case ReasonToFinishPlaying.StoppedByUser:
                    this.lastMessage = "Video was stopped";
                    break;

                case ReasonToFinishPlaying.DeviceLost:
                    this.lastMessage = "Video device was unplugged";
                    break;

                case ReasonToFinishPlaying.VideoSourceError:
                    this.lastMessage = "Video has finished because of error in video source";
                    break;

                default:
                    this.lastMessage = "Video has finished for unknown reason";
                    break;
            }
            base.Invalidate();
            if (this.PlayingFinished != null)
            {
                this.PlayingFinished(this, reason);
            }
        }

        private void videoSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            this.lastMessage = eventArgs.Description;
            base.Invalidate();
        }

        private void VideoSourcePlayer_Paint(object sender, PaintEventArgs e)
        {
            if (this.needSizeUpdate || this.firstFrameNotProcessed)
            {
                this.UpdatePosition();
                this.needSizeUpdate = false;
            }
            lock (this.sync)
            {
                Graphics graphics = e.Graphics;
                Rectangle clientRectangle = base.ClientRectangle;
                Pen pen = new Pen(this.borderColor, 1f);
                graphics.DrawRectangle(pen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
                if (this.videoSource != null)
                {
                    if ((this.currentFrame != null) && (this.lastMessage == null))
                    {
                        graphics.DrawImage((this.convertedFrame != null) ? this.convertedFrame : this.currentFrame, (int) (clientRectangle.X + 1), (int) (clientRectangle.Y + 1), (int) (clientRectangle.Width - 2), (int) (clientRectangle.Height - 2));
                        this.firstFrameNotProcessed = false;
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(this.ForeColor);
                        graphics.DrawString((this.lastMessage == null) ? "Connecting ..." : this.lastMessage, this.Font, brush, new PointF(5f, 5f));
                        brush.Dispose();
                    }
                }
                pen.Dispose();
            }
        }

        private void VideoSourcePlayer_ParentChanged(object sender, EventArgs e)
        {
            if (this.parent != null)
            {
                this.parent.SizeChanged -= new EventHandler(this.parent_SizeChanged);
            }
            this.parent = base.Parent;
            if (this.parent != null)
            {
                this.parent.SizeChanged += new EventHandler(this.parent_SizeChanged);
            }
        }

        public void WaitForStop()
        {
            this.CheckForCrossThreadAccess();
            if (!this.requestedToStop)
            {
                this.SignalToStop();
            }
            if (this.videoSource != null)
            {
                this.videoSource.WaitForStop();
                if (this.currentFrame != null)
                {
                    this.currentFrame.Dispose();
                    this.currentFrame = null;
                }
                base.Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool AutoSizeControl
        {
            get
            {
                return this.autosize;
            }
            set
            {
                this.autosize = value;
                this.UpdatePosition();
            }
        }

        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }
            set
            {
                this.borderColor = value;
                base.Invalidate();
            }
        }

        [Browsable(false)]
        public bool IsRunning
        {
            get
            {
                this.CheckForCrossThreadAccess();
                if (this.videoSource == null)
                {
                    return false;
                }
                return this.videoSource.IsRunning;
            }
        }

        [Browsable(false)]
        public IVideoSource VideoSource
        {
            get
            {
                return this.videoSource;
            }
            set
            {
                this.CheckForCrossThreadAccess();
                if (this.videoSource != null)
                {
                    this.videoSource.NewFrame -= new NewFrameEventHandler(this.videoSource_NewFrame);
                    this.videoSource.VideoSourceError -= new VideoSourceErrorEventHandler(this.videoSource_VideoSourceError);
                    this.videoSource.PlayingFinished -= new PlayingFinishedEventHandler(this.videoSource_PlayingFinished);
                }
                lock (this.sync)
                {
                    if (this.currentFrame != null)
                    {
                        this.currentFrame.Dispose();
                        this.currentFrame = null;
                    }
                }
                this.videoSource = value;
                if (this.videoSource != null)
                {
                    this.videoSource.NewFrame += new NewFrameEventHandler(this.videoSource_NewFrame);
                    this.videoSource.VideoSourceError += new VideoSourceErrorEventHandler(this.videoSource_VideoSourceError);
                    this.videoSource.PlayingFinished += new PlayingFinishedEventHandler(this.videoSource_PlayingFinished);
                }
                else
                {
                    this.frameSize = new Size(320, 240);
                }
                this.lastMessage = null;
                this.needSizeUpdate = true;
                this.firstFrameNotProcessed = true;
                base.Invalidate();
            }
        }

        public delegate void NewFrameHandler(object sender, ref Bitmap image);
    }
}

