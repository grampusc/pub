namespace VideoCapture.Video.DirectShow
{
    using VideoCapture.Video;
    using VideoCapture.Video.DirectShow.Internals;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class VideoCaptureDevice : IVideoSource
    {
        private long bytesReceived;
        private static Dictionary<string, VideoInput[]> cacheCrossbarVideoInputs = new Dictionary<string, VideoInput[]>();
        private static Dictionary<string, Boka.VideoCapture.Video.DirectShow.VideoCapabilities[]> cacheSnapshotCapabilities = new Dictionary<string, Boka.VideoCapture.Video.DirectShow.VideoCapabilities[]>();
        private static Dictionary<string, Boka.VideoCapture.Video.DirectShow.VideoCapabilities[]> cacheVideoCapabilities = new Dictionary<string, Boka.VideoCapture.Video.DirectShow.VideoCapabilities[]>();
        private VideoInput crossbarVideoInput;
        private VideoInput[] crossbarVideoInputs;
        private string deviceMoniker;
        private int framesReceived;
        private bool? isCrossbarAvailable;
        private bool needToDisplayCrossBarPropertyPage;
        private bool needToDisplayPropertyPage;
        private bool needToSetVideoInput;
        private bool needToSimulateTrigger;
        private NewFrameEventHandler NewFrame;
        private IntPtr parentWindowForPropertyPage;
        private PlayingFinishedEventHandler PlayingFinished;
        private bool provideSnapshots;
        private Boka.VideoCapture.Video.DirectShow.VideoCapabilities[] snapshotCapabilities;
        private NewFrameEventHandler SnapshotFrame;
        private Boka.VideoCapture.Video.DirectShow.VideoCapabilities snapshotResolution;
        private object sourceObject;
        private DateTime startTime;
        private ManualResetEvent stopEvent;
        private object sync;
        private Thread thread;
        private Boka.VideoCapture.Video.DirectShow.VideoCapabilities[] videoCapabilities;
        private Boka.VideoCapture.Video.DirectShow.VideoCapabilities videoResolution;
        private VideoSourceErrorEventHandler VideoSourceError;

        public event NewFrameEventHandler NewFrame
        {
            add
            {
                NewFrameEventHandler handler2;
                NewFrameEventHandler newFrame = this.NewFrame;
                do
                {
                    handler2 = newFrame;
                    NewFrameEventHandler handler3 = (NewFrameEventHandler) Delegate.Combine(handler2, value);
                    newFrame = Interlocked.CompareExchange<NewFrameEventHandler>(ref this.NewFrame, handler3, handler2);
                }
                while (newFrame != handler2);
            }
            remove
            {
                NewFrameEventHandler handler2;
                NewFrameEventHandler newFrame = this.NewFrame;
                do
                {
                    handler2 = newFrame;
                    NewFrameEventHandler handler3 = (NewFrameEventHandler) Delegate.Remove(handler2, value);
                    newFrame = Interlocked.CompareExchange<NewFrameEventHandler>(ref this.NewFrame, handler3, handler2);
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

        public event NewFrameEventHandler SnapshotFrame
        {
            add
            {
                NewFrameEventHandler handler2;
                NewFrameEventHandler snapshotFrame = this.SnapshotFrame;
                do
                {
                    handler2 = snapshotFrame;
                    NewFrameEventHandler handler3 = (NewFrameEventHandler) Delegate.Combine(handler2, value);
                    snapshotFrame = Interlocked.CompareExchange<NewFrameEventHandler>(ref this.SnapshotFrame, handler3, handler2);
                }
                while (snapshotFrame != handler2);
            }
            remove
            {
                NewFrameEventHandler handler2;
                NewFrameEventHandler snapshotFrame = this.SnapshotFrame;
                do
                {
                    handler2 = snapshotFrame;
                    NewFrameEventHandler handler3 = (NewFrameEventHandler) Delegate.Remove(handler2, value);
                    snapshotFrame = Interlocked.CompareExchange<NewFrameEventHandler>(ref this.SnapshotFrame, handler3, handler2);
                }
                while (snapshotFrame != handler2);
            }
        }

        public event VideoSourceErrorEventHandler VideoSourceError
        {
            add
            {
                VideoSourceErrorEventHandler handler2;
                VideoSourceErrorEventHandler videoSourceError = this.VideoSourceError;
                do
                {
                    handler2 = videoSourceError;
                    VideoSourceErrorEventHandler handler3 = (VideoSourceErrorEventHandler) Delegate.Combine(handler2, value);
                    videoSourceError = Interlocked.CompareExchange<VideoSourceErrorEventHandler>(ref this.VideoSourceError, handler3, handler2);
                }
                while (videoSourceError != handler2);
            }
            remove
            {
                VideoSourceErrorEventHandler handler2;
                VideoSourceErrorEventHandler videoSourceError = this.VideoSourceError;
                do
                {
                    handler2 = videoSourceError;
                    VideoSourceErrorEventHandler handler3 = (VideoSourceErrorEventHandler) Delegate.Remove(handler2, value);
                    videoSourceError = Interlocked.CompareExchange<VideoSourceErrorEventHandler>(ref this.VideoSourceError, handler3, handler2);
                }
                while (videoSourceError != handler2);
            }
        }

        public VideoCaptureDevice()
        {
            this.parentWindowForPropertyPage = IntPtr.Zero;
            this.startTime = new DateTime();
            this.sync = new object();
            this.isCrossbarAvailable = null;
            this.crossbarVideoInput = VideoInput.Default;
        }

        public VideoCaptureDevice(string deviceMoniker)
        {
            this.parentWindowForPropertyPage = IntPtr.Zero;
            this.startTime = new DateTime();
            this.sync = new object();
            this.isCrossbarAvailable = null;
            this.crossbarVideoInput = VideoInput.Default;
            this.deviceMoniker = deviceMoniker;
        }

        public bool CheckIfCrossbarAvailable()
        {
            lock (this.sync)
            {
                if (!this.isCrossbarAvailable.HasValue)
                {
                    if (!this.IsRunning)
                    {
                        this.WorkerThread(false);
                    }
                    else
                    {
                        for (int i = 0; (i < 500) && !this.isCrossbarAvailable.HasValue; i++)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                return (!this.isCrossbarAvailable.HasValue ? false : this.isCrossbarAvailable.Value);
            }
        }

        private VideoInput[] ColletCrossbarVideoInputs(IAMCrossbar crossbar)
        {
            lock (cacheCrossbarVideoInputs)
            {
                int num;
                int num2;
                if (cacheCrossbarVideoInputs.ContainsKey(this.deviceMoniker))
                {
                    return cacheCrossbarVideoInputs[this.deviceMoniker];
                }
                List<VideoInput> list = new List<VideoInput>();
                if ((crossbar != null) && (crossbar.get_PinCounts(out num2, out num) == 0))
                {
                    for (int i = 0; i < num; i++)
                    {
                        int num4;
                        PhysicalConnectorType type;
                        if ((crossbar.get_CrossbarPinInfo(true, i, out num4, out type) == 0) && (type < PhysicalConnectorType.AudioTuner))
                        {
                            list.Add(new VideoInput(i, type));
                        }
                    }
                }
                VideoInput[] array = new VideoInput[list.Count];
                list.CopyTo(array);
                cacheCrossbarVideoInputs.Add(this.deviceMoniker, array);
                return array;
            }
        }

        public void DisplayCrossbarPropertyPage(IntPtr parentWindow)
        {
            lock (this.sync)
            {
                for (int i = 0; ((i < 500) && !this.isCrossbarAvailable.HasValue) && this.IsRunning; i++)
                {
                    Thread.Sleep(10);
                }
                if (!(this.IsRunning && this.isCrossbarAvailable.HasValue))
                {
                    throw new ApplicationException("The video source must be running in order to display crossbar property page.");
                }
                if (!this.isCrossbarAvailable.Value)
                {
                    throw new NotSupportedException("Crossbar configuration is not supported by currently running video source.");
                }
                this.parentWindowForPropertyPage = parentWindow;
                this.needToDisplayCrossBarPropertyPage = true;
            }
        }

        public void DisplayPropertyPage(IntPtr parentWindow)
        {
            if ((this.deviceMoniker == null) || (this.deviceMoniker == string.Empty))
            {
                throw new ArgumentException("Video source is not specified.");
            }
            lock (this.sync)
            {
                if (this.IsRunning)
                {
                    this.parentWindowForPropertyPage = parentWindow;
                    this.needToDisplayPropertyPage = true;
                }
                else
                {
                    object sourceObject = null;
                    try
                    {
                        sourceObject = Boka.VideoCapture.Video.DirectShow.FilterInfo.CreateFilter(this.deviceMoniker);
                    }
                    catch
                    {
                        throw new ApplicationException("Failed creating device object for moniker.");
                    }
                    if (!(sourceObject is ISpecifyPropertyPages))
                    {
                        throw new NotSupportedException("The video source does not support configuration property page.");
                    }
                    this.DisplayPropertyPage(parentWindow, sourceObject);
                    Marshal.ReleaseComObject(sourceObject);
                }
            }
        }

        private void DisplayPropertyPage(IntPtr parentWindow, object sourceObject)
        {
            try
            {
                CAUUID cauuid;
                ((ISpecifyPropertyPages) sourceObject).GetPages(out cauuid);
                Boka.VideoCapture.Video.DirectShow.FilterInfo info = new Boka.VideoCapture.Video.DirectShow.FilterInfo(this.deviceMoniker);
                Win32.OleCreatePropertyFrame(parentWindow, 0, 0, info.Name, 1, ref sourceObject, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero);
                Marshal.FreeCoTaskMem(cauuid.pElems);
            }
            catch
            {
            }
        }

        private void Free()
        {
            this.thread = null;
            this.stopEvent.Close();
            this.stopEvent = null;
        }

        public bool GetCameraProperty(CameraControlProperty property, out int value, out CameraControlFlags controlFlags)
        {
            bool flag = true;
            if ((this.deviceMoniker == null) || string.IsNullOrEmpty(this.deviceMoniker))
            {
                throw new ArgumentException("Video source is not specified.");
            }
            lock (this.sync)
            {
                object o = null;
                try
                {
                    o = Boka.VideoCapture.Video.DirectShow.FilterInfo.CreateFilter(this.deviceMoniker);
                }
                catch
                {
                    throw new ApplicationException("Failed creating device object for moniker.");
                }
                if (!(o is IAMCameraControl))
                {
                    throw new NotSupportedException("The video source does not support camera control.");
                }
                IAMCameraControl control = (IAMCameraControl) o;
                flag = control.Get(property, out value, out controlFlags) >= 0;
                Marshal.ReleaseComObject(o);
            }
            return flag;
        }

        public bool GetCameraPropertyRange(CameraControlProperty property, out int minValue, out int maxValue, out int stepSize, out int defaultValue, out CameraControlFlags controlFlags)
        {
            bool flag = true;
            if ((this.deviceMoniker == null) || string.IsNullOrEmpty(this.deviceMoniker))
            {
                throw new ArgumentException("Video source is not specified.");
            }
            lock (this.sync)
            {
                object o = null;
                try
                {
                    o = Boka.VideoCapture.Video.DirectShow.FilterInfo.CreateFilter(this.deviceMoniker);
                }
                catch
                {
                    throw new ApplicationException("Failed creating device object for moniker.");
                }
                if (!(o is IAMCameraControl))
                {
                    throw new NotSupportedException("The video source does not support camera control.");
                }
                IAMCameraControl control = (IAMCameraControl) o;
                flag = control.GetRange(property, out minValue, out maxValue, out stepSize, out defaultValue, out controlFlags) >= 0;
                Marshal.ReleaseComObject(o);
            }
            return flag;
        }

        private VideoInput GetCurrentCrossbarInput(IAMCrossbar crossbar)
        {
            int num;
            int num2;
            VideoInput input = VideoInput.Default;
            if (crossbar.get_PinCounts(out num2, out num) == 0)
            {
                int num3;
                int num4;
                int outputPinIndex = -1;
                for (int i = 0; i < num2; i++)
                {
                    PhysicalConnectorType type;
                    if ((crossbar.get_CrossbarPinInfo(false, i, out num3, out type) == 0) && (type == PhysicalConnectorType.VideoDecoder))
                    {
                        outputPinIndex = i;
                        break;
                    }
                }
                if ((outputPinIndex != -1) && (crossbar.get_IsRoutedTo(outputPinIndex, out num4) == 0))
                {
                    PhysicalConnectorType type2;
                    crossbar.get_CrossbarPinInfo(true, num4, out num3, out type2);
                    input = new VideoInput(num4, type2);
                }
            }
            return input;
        }

        private void GetPinCapabilitiesAndConfigureSizeAndRate(ICaptureGraphBuilder2 graphBuilder, IBaseFilter baseFilter, Guid pinCategory, Boka.VideoCapture.Video.DirectShow.VideoCapabilities resolutionToSet, ref Boka.VideoCapture.Video.DirectShow.VideoCapabilities[] capabilities)
        {
            object obj2;
            graphBuilder.FindInterface(pinCategory, MediaType.Video, baseFilter, typeof(IAMStreamConfig).GUID, out obj2);
            if (obj2 != null)
            {
                IAMStreamConfig videoStreamConfig = null;
                try
                {
                    videoStreamConfig = (IAMStreamConfig) obj2;
                }
                catch (InvalidCastException)
                {
                }
                if (videoStreamConfig != null)
                {
                    if (capabilities == null)
                    {
                        try
                        {
                            capabilities = Boka.VideoCapture.Video.DirectShow.VideoCapabilities.FromStreamConfig(videoStreamConfig);
                        }
                        catch
                        {
                        }
                    }
                    if (resolutionToSet != null)
                    {
                        this.SetResolution(videoStreamConfig, resolutionToSet);
                    }
                }
            }
            if (capabilities == null)
            {
                capabilities = new Boka.VideoCapture.Video.DirectShow.VideoCapabilities[0];
            }
        }

        private void OnNewFrame(Bitmap image)
        {
            this.framesReceived++;
            this.bytesReceived += (image.Width * image.Height) * (Image.GetPixelFormatSize(image.PixelFormat) >> 3);
            if (!(this.stopEvent.WaitOne(0, false) || (this.NewFrame == null)))
            {
                this.NewFrame(this, new NewFrameEventArgs(image));
            }
        }

        private void OnSnapshotFrame(Bitmap image)
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - this.startTime);
            if (((span.TotalSeconds >= 4.0) && !this.stopEvent.WaitOne(0, false)) && (this.SnapshotFrame != null))
            {
                this.SnapshotFrame(this, new NewFrameEventArgs(image));
            }
        }

        public bool SetCameraProperty(CameraControlProperty property, int value, CameraControlFlags controlFlags)
        {
            bool flag = true;
            if ((this.deviceMoniker == null) || string.IsNullOrEmpty(this.deviceMoniker))
            {
                throw new ArgumentException("Video source is not specified.");
            }
            lock (this.sync)
            {
                object o = null;
                try
                {
                    o = Boka.VideoCapture.Video.DirectShow.FilterInfo.CreateFilter(this.deviceMoniker);
                }
                catch
                {
                    throw new ApplicationException("Failed creating device object for moniker.");
                }
                if (!(o is IAMCameraControl))
                {
                    throw new NotSupportedException("The video source does not support camera control.");
                }
                IAMCameraControl control = (IAMCameraControl) o;
                flag = control.Set(property, value, controlFlags) >= 0;
                Marshal.ReleaseComObject(o);
            }
            return flag;
        }

        private void SetCurrentCrossbarInput(IAMCrossbar crossbar, VideoInput videoInput)
        {
            int num;
            int num2;
            if ((videoInput.Type != PhysicalConnectorType.Default) && (crossbar.get_PinCounts(out num2, out num) == 0))
            {
                int num3;
                PhysicalConnectorType type;
                int outputPinIndex = -1;
                int inputPinIndex = -1;
                for (int i = 0; i < num2; i++)
                {
                    if ((crossbar.get_CrossbarPinInfo(false, i, out num3, out type) == 0) && (type == PhysicalConnectorType.VideoDecoder))
                    {
                        outputPinIndex = i;
                        break;
                    }
                }
                for (int j = 0; j < num; j++)
                {
                    if (((crossbar.get_CrossbarPinInfo(true, j, out num3, out type) == 0) && (type == videoInput.Type)) && (j == videoInput.Index))
                    {
                        inputPinIndex = j;
                        break;
                    }
                }
                if (((inputPinIndex != -1) && (outputPinIndex != -1)) && (crossbar.CanRoute(outputPinIndex, inputPinIndex) == 0))
                {
                    crossbar.Route(outputPinIndex, inputPinIndex);
                }
            }
        }

        private void SetResolution(IAMStreamConfig streamConfig, Boka.VideoCapture.Video.DirectShow.VideoCapabilities resolution)
        {
            if (resolution != null)
            {
                int count = 0;
                int size = 0;
                AMMediaType mediaType = null;
                VideoStreamConfigCaps streamConfigCaps = new VideoStreamConfigCaps();
                streamConfig.GetNumberOfCapabilities(out count, out size);
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        Boka.VideoCapture.Video.DirectShow.VideoCapabilities capabilities = new Boka.VideoCapture.Video.DirectShow.VideoCapabilities(streamConfig, i);
                        if ((resolution == capabilities) && (streamConfig.GetStreamCaps(i, out mediaType, streamConfigCaps) == 0))
                        {
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
                if (mediaType != null)
                {
                    streamConfig.SetFormat(mediaType);
                    mediaType.Dispose();
                }
            }
        }

        public void SignalToStop()
        {
            if (this.thread != null)
            {
                this.stopEvent.Set();
            }
        }

        public void SimulateTrigger()
        {
            this.needToSimulateTrigger = true;
        }

        public void Start()
        {
            if (!this.IsRunning)
            {
                if (string.IsNullOrEmpty(this.deviceMoniker))
                {
                    throw new ArgumentException("Video source is not specified.");
                }
                this.framesReceived = 0;
                this.bytesReceived = 0L;
                this.isCrossbarAvailable = null;
                this.needToSetVideoInput = true;
                this.stopEvent = new ManualResetEvent(false);
                lock (this.sync)
                {
                    this.thread = new Thread(new ThreadStart(this.WorkerThread));
                    this.thread.Name = this.deviceMoniker;
                    this.thread.Start();
                }
            }
        }

        public void Stop()
        {
            if (this.IsRunning)
            {
                this.thread.Abort();
                this.WaitForStop();
            }
        }

        public void WaitForStop()
        {
            if (this.thread != null)
            {
                this.thread.Join();
                this.Free();
            }
        }

        private void WorkerThread()
        {
            this.WorkerThread(true);
        }

        private void WorkerThread(bool runGraph)
        {
            ReasonToFinishPlaying stoppedByUser = ReasonToFinishPlaying.StoppedByUser;
            bool flag = false;
            Grabber callback = new Grabber(this, false);
            Grabber grabber2 = new Grabber(this, true);
            object o = null;
            object obj3 = null;
            object obj4 = null;
            object obj5 = null;
            object retInterface = null;
            ICaptureGraphBuilder2 graphBuilder = null;
            IFilterGraph2 graph = null;
            IBaseFilter sourceObject = null;
            IBaseFilter filter2 = null;
            IBaseFilter filter3 = null;
            ISampleGrabber grabber3 = null;
            ISampleGrabber grabber4 = null;
            IMediaControl control = null;
            IAMVideoControl control2 = null;
            IMediaEventEx ex = null;
            IPin pin = null;
            IAMCrossbar crossbar = null;
            try
            {
                Dictionary<string, Boka.VideoCapture.Video.DirectShow.VideoCapabilities[]> dictionary;
                Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
                if (typeFromCLSID == null)
                {
                    throw new ApplicationException("Failed creating capture graph builder");
                }
                o = Activator.CreateInstance(typeFromCLSID);
                graphBuilder = (ICaptureGraphBuilder2) o;
                typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
                if (typeFromCLSID == null)
                {
                    throw new ApplicationException("Failed creating filter graph");
                }
                obj3 = Activator.CreateInstance(typeFromCLSID);
                graph = (IFilterGraph2) obj3;
                graphBuilder.SetFiltergraph((IGraphBuilder) graph);
                this.sourceObject = Boka.VideoCapture.Video.DirectShow.FilterInfo.CreateFilter(this.deviceMoniker);
                if (this.sourceObject == null)
                {
                    throw new ApplicationException("Failed creating device object for moniker");
                }
                sourceObject = (IBaseFilter) this.sourceObject;
                try
                {
                    control2 = (IAMVideoControl) this.sourceObject;
                }
                catch
                {
                }
                typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
                if (typeFromCLSID == null)
                {
                    throw new ApplicationException("Failed creating sample grabber");
                }
                obj4 = Activator.CreateInstance(typeFromCLSID);
                grabber3 = (ISampleGrabber) obj4;
                filter2 = (IBaseFilter) obj4;
                obj5 = Activator.CreateInstance(typeFromCLSID);
                grabber4 = (ISampleGrabber) obj5;
                filter3 = (IBaseFilter) obj5;
                graph.AddFilter(sourceObject, "source");
                graph.AddFilter(filter2, "grabber_video");
                graph.AddFilter(filter3, "grabber_snapshot");
                AMMediaType mediaType = new AMMediaType();
                mediaType.MajorType = MediaType.Video;
                mediaType.SubType = MediaSubType.RGB24;
                grabber3.SetMediaType(mediaType);
                grabber4.SetMediaType(mediaType);
                graphBuilder.FindInterface(FindDirection.UpstreamOnly, Guid.Empty, sourceObject, typeof(IAMCrossbar).GUID, out retInterface);
                if (retInterface != null)
                {
                    crossbar = (IAMCrossbar) retInterface;
                }
                this.isCrossbarAvailable = new bool?(crossbar != null);
                this.crossbarVideoInputs = this.ColletCrossbarVideoInputs(crossbar);
                if (control2 != null)
                {
                    graphBuilder.FindPin(this.sourceObject, PinDirection.Output, PinCategory.StillImage, MediaType.Video, false, 0, out pin);
                    if (pin != null)
                    {
                        VideoControlFlags flags;
                        control2.GetCaps(pin, out flags);
                        flag = (flags & VideoControlFlags.ExternalTriggerEnable) != 0;
                    }
                }
                grabber3.SetBufferSamples(false);
                grabber3.SetOneShot(false);
                grabber3.SetCallback(callback, 1);
                grabber4.SetBufferSamples(true);
                grabber4.SetOneShot(false);
                grabber4.SetCallback(grabber2, 1);
                this.GetPinCapabilitiesAndConfigureSizeAndRate(graphBuilder, sourceObject, PinCategory.Capture, this.videoResolution, ref this.videoCapabilities);
                if (flag)
                {
                    this.GetPinCapabilitiesAndConfigureSizeAndRate(graphBuilder, sourceObject, PinCategory.StillImage, this.snapshotResolution, ref this.snapshotCapabilities);
                }
                else
                {
                    this.snapshotCapabilities = new Boka.VideoCapture.Video.DirectShow.VideoCapabilities[0];
                }
                lock ((dictionary = cacheVideoCapabilities))
                {
                    if (!((this.videoCapabilities == null) || cacheVideoCapabilities.ContainsKey(this.deviceMoniker)))
                    {
                        cacheVideoCapabilities.Add(this.deviceMoniker, this.videoCapabilities);
                    }
                }
                lock ((dictionary = cacheSnapshotCapabilities))
                {
                    if (!((this.snapshotCapabilities == null) || cacheSnapshotCapabilities.ContainsKey(this.deviceMoniker)))
                    {
                        cacheSnapshotCapabilities.Add(this.deviceMoniker, this.snapshotCapabilities);
                    }
                }
                if (runGraph)
                {
                    graphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, sourceObject, null, filter2);
                    if (grabber3.GetConnectedMediaType(mediaType) == 0)
                    {
                        VideoInfoHeader header = (VideoInfoHeader) Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                        callback.Width = header.BmiHeader.Width;
                        callback.Height = header.BmiHeader.Height;
                        mediaType.Dispose();
                    }
                    if (flag && this.provideSnapshots)
                    {
                        graphBuilder.RenderStream(PinCategory.StillImage, MediaType.Video, sourceObject, null, filter3);
                        if (grabber4.GetConnectedMediaType(mediaType) == 0)
                        {
                            VideoInfoHeader header2 = (VideoInfoHeader) Marshal.PtrToStructure(mediaType.FormatPtr, typeof(VideoInfoHeader));
                            grabber2.Width = header2.BmiHeader.Width;
                            grabber2.Height = header2.BmiHeader.Height;
                            mediaType.Dispose();
                        }
                    }
                    control = (IMediaControl) obj3;
                    ex = (IMediaEventEx) obj3;
                    control.Run();
                    if (flag && this.provideSnapshots)
                    {
                        this.startTime = DateTime.Now;
                        control2.SetMode(pin, VideoControlFlags.ExternalTriggerEnable);
                    }
                    do
                    {
                        IntPtr ptr;
                        IntPtr ptr2;
                        DsEvCode code;
                        if ((ex != null) && (ex.GetEvent(out code, out ptr, out ptr2, 0) >= 0))
                        {
                            ex.FreeEventParams(code, ptr, ptr2);
                            if (code == DsEvCode.DeviceLost)
                            {
                                stoppedByUser = ReasonToFinishPlaying.DeviceLost;
                                break;
                            }
                        }
                        if (this.needToSetVideoInput)
                        {
                            this.needToSetVideoInput = false;
                            if (this.isCrossbarAvailable.Value)
                            {
                                this.SetCurrentCrossbarInput(crossbar, this.crossbarVideoInput);
                                this.crossbarVideoInput = this.GetCurrentCrossbarInput(crossbar);
                            }
                        }
                        if (this.needToSimulateTrigger)
                        {
                            this.needToSimulateTrigger = false;
                            if (flag && this.provideSnapshots)
                            {
                                control2.SetMode(pin, VideoControlFlags.Trigger);
                            }
                        }
                        if (this.needToDisplayPropertyPage)
                        {
                            this.needToDisplayPropertyPage = false;
                            this.DisplayPropertyPage(this.parentWindowForPropertyPage, this.sourceObject);
                            if (crossbar != null)
                            {
                                this.crossbarVideoInput = this.GetCurrentCrossbarInput(crossbar);
                            }
                        }
                        if (this.needToDisplayCrossBarPropertyPage)
                        {
                            this.needToDisplayCrossBarPropertyPage = false;
                            if (crossbar != null)
                            {
                                this.DisplayPropertyPage(this.parentWindowForPropertyPage, crossbar);
                                this.crossbarVideoInput = this.GetCurrentCrossbarInput(crossbar);
                            }
                        }
                    }
                    while (!this.stopEvent.WaitOne(100, false));
                    control.Stop();
                }
            }
            catch (Exception exception)
            {
                if (this.VideoSourceError != null)
                {
                    this.VideoSourceError(this, new VideoSourceErrorEventArgs(exception.Message));
                }
            }
            finally
            {
                graphBuilder = null;
                graph = null;
                sourceObject = null;
                control = null;
                control2 = null;
                ex = null;
                pin = null;
                crossbar = null;
                filter2 = null;
                filter3 = null;
                grabber3 = null;
                grabber4 = null;
                if (obj3 != null)
                {
                    Marshal.ReleaseComObject(obj3);
                    obj3 = null;
                }
                if (this.sourceObject != null)
                {
                    Marshal.ReleaseComObject(this.sourceObject);
                    this.sourceObject = null;
                }
                if (obj4 != null)
                {
                    Marshal.ReleaseComObject(obj4);
                    obj4 = null;
                }
                if (obj5 != null)
                {
                    Marshal.ReleaseComObject(obj5);
                    obj5 = null;
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                    o = null;
                }
                if (retInterface != null)
                {
                    Marshal.ReleaseComObject(retInterface);
                    retInterface = null;
                }
            }
            if (this.PlayingFinished != null)
            {
                this.PlayingFinished(this, stoppedByUser);
            }
        }

        public VideoInput[] AvailableCrossbarVideoInputs
        {
            get
            {
                if (this.crossbarVideoInputs == null)
                {
                    lock (cacheCrossbarVideoInputs)
                    {
                        if (!(string.IsNullOrEmpty(this.deviceMoniker) || !cacheCrossbarVideoInputs.ContainsKey(this.deviceMoniker)))
                        {
                            this.crossbarVideoInputs = cacheCrossbarVideoInputs[this.deviceMoniker];
                        }
                    }
                    if (this.crossbarVideoInputs == null)
                    {
                        if (!this.IsRunning)
                        {
                            this.WorkerThread(false);
                        }
                        else
                        {
                            for (int i = 0; (i < 500) && (this.crossbarVideoInputs == null); i++)
                            {
                                Thread.Sleep(10);
                            }
                        }
                    }
                }
                if (this.crossbarVideoInputs == null)
                {
                    return new VideoInput[0];
                }
                return this.crossbarVideoInputs;
            }
        }

        public long BytesReceived
        {
            get
            {
                long bytesReceived = this.bytesReceived;
                this.bytesReceived = 0L;
                return bytesReceived;
            }
        }

        public VideoInput CrossbarVideoInput
        {
            get
            {
                return this.crossbarVideoInput;
            }
            set
            {
                this.needToSetVideoInput = true;
                this.crossbarVideoInput = value;
            }
        }

        [Obsolete]
        public int DesiredFrameRate
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        [Obsolete]
        public Size DesiredFrameSize
        {
            get
            {
                return Size.Empty;
            }
            set
            {
            }
        }

        [Obsolete]
        public Size DesiredSnapshotSize
        {
            get
            {
                return Size.Empty;
            }
            set
            {
            }
        }

        public int FramesReceived
        {
            get
            {
                int framesReceived = this.framesReceived;
                this.framesReceived = 0;
                return framesReceived;
            }
        }

        public bool IsRunning
        {
            get
            {
                if (this.thread != null)
                {
                    if (!this.thread.Join(0))
                    {
                        return true;
                    }
                    this.Free();
                }
                return false;
            }
        }

        public bool ProvideSnapshots
        {
            get
            {
                return this.provideSnapshots;
            }
            set
            {
                this.provideSnapshots = value;
            }
        }

        public Boka.VideoCapture.Video.DirectShow.VideoCapabilities[] SnapshotCapabilities
        {
            get
            {
                if (this.snapshotCapabilities == null)
                {
                    lock (cacheSnapshotCapabilities)
                    {
                        if (!(string.IsNullOrEmpty(this.deviceMoniker) || !cacheSnapshotCapabilities.ContainsKey(this.deviceMoniker)))
                        {
                            this.snapshotCapabilities = cacheSnapshotCapabilities[this.deviceMoniker];
                        }
                    }
                    if (this.snapshotCapabilities == null)
                    {
                        if (!this.IsRunning)
                        {
                            this.WorkerThread(false);
                        }
                        else
                        {
                            for (int i = 0; (i < 500) && (this.snapshotCapabilities == null); i++)
                            {
                                Thread.Sleep(10);
                            }
                        }
                    }
                }
                if (this.snapshotCapabilities == null)
                {
                    return new Boka.VideoCapture.Video.DirectShow.VideoCapabilities[0];
                }
                return this.snapshotCapabilities;
            }
        }

        public Boka.VideoCapture.Video.DirectShow.VideoCapabilities SnapshotResolution
        {
            get
            {
                return this.snapshotResolution;
            }
            set
            {
                this.snapshotResolution = value;
            }
        }

        public virtual string Source
        {
            get
            {
                return this.deviceMoniker;
            }
            set
            {
                this.deviceMoniker = value;
                this.videoCapabilities = null;
                this.snapshotCapabilities = null;
                this.crossbarVideoInputs = null;
                this.isCrossbarAvailable = null;
            }
        }

        public object SourceObject
        {
            get
            {
                return this.sourceObject;
            }
        }

        public Boka.VideoCapture.Video.DirectShow.VideoCapabilities[] VideoCapabilities
        {
            get
            {
                if (this.videoCapabilities == null)
                {
                    lock (cacheVideoCapabilities)
                    {
                        if (!(string.IsNullOrEmpty(this.deviceMoniker) || !cacheVideoCapabilities.ContainsKey(this.deviceMoniker)))
                        {
                            this.videoCapabilities = cacheVideoCapabilities[this.deviceMoniker];
                        }
                    }
                    if (this.videoCapabilities == null)
                    {
                        if (!this.IsRunning)
                        {
                            this.WorkerThread(false);
                        }
                        else
                        {
                            for (int i = 0; (i < 500) && (this.videoCapabilities == null); i++)
                            {
                                Thread.Sleep(10);
                            }
                        }
                    }
                }
                if (this.videoCapabilities == null)
                {
                    return new Boka.VideoCapture.Video.DirectShow.VideoCapabilities[0];
                }
                return this.videoCapabilities;
            }
        }

        public Boka.VideoCapture.Video.DirectShow.VideoCapabilities VideoResolution
        {
            get
            {
                return this.videoResolution;
            }
            set
            {
                this.videoResolution = value;
            }
        }

        private class Grabber : ISampleGrabberCB
        {
            private int height;
            private VideoCaptureDevice parent;
            private bool snapshotMode;
            private int width;

            public Grabber(VideoCaptureDevice parent, bool snapshotMode)
            {
                this.parent = parent;
                this.snapshotMode = snapshotMode;
            }

            public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
            {
                if (this.parent.NewFrame != null)
                {
                    Bitmap image = new Bitmap(this.width, this.height, PixelFormat.Format24bppRgb);
                    BitmapData bitmapdata = image.LockBits(new Rectangle(0, 0, this.width, this.height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int stride = bitmapdata.Stride;
                    int num2 = bitmapdata.Stride;
                    byte* dst = (byte*) (bitmapdata.Scan0.ToPointer() + (num2 * (this.height - 1)));
                    byte* src = (byte*) buffer.ToPointer();
                    for (int i = 0; i < this.height; i++)
                    {
                        Win32.memcpy(dst, src, stride);
                        dst -= num2;
                        src += stride;
                    }
                    image.UnlockBits(bitmapdata);
                    if (this.snapshotMode)
                    {
                        this.parent.OnSnapshotFrame(image);
                    }
                    else
                    {
                        this.parent.OnNewFrame(image);
                    }
                    image.Dispose();
                }
                return 0;
            }

            public int SampleCB(double sampleTime, IntPtr sample)
            {
                return 0;
            }

            public int Height
            {
                get
                {
                    return this.height;
                }
                set
                {
                    this.height = value;
                }
            }

            public int Width
            {
                get
                {
                    return this.width;
                }
                set
                {
                    this.width = value;
                }
            }
        }
    }
}

