using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Extensions;
using static Emgu.CV.VideoCapture;

namespace view_models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ICommand _captureImageCommand;
        private ICommand _windowClosingCommand;
        private bool _showLivestream;
        private int _preferredFramerate = 60;
        private VideoCapture capture;

        public MainViewModel()
        {
            capture = new VideoCapture();
            capture.ImageGrabbed += Capture_ImageGrabbed;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource ImageSource { get; set; } = new BitmapImage();

        public string Device { get; set; }

        public ICommand CaptureImageCommand
        {
            get
            {
                if (_captureImageCommand == null)
                {
                    _captureImageCommand = new RelayCommand(() => CaptureImage());
                }

                return _captureImageCommand;
            }
        }

        public ICommand WindowClosingCommand
        {
            get
            {
                if (_windowClosingCommand == null)
                {
                    _windowClosingCommand = new RelayCommand(() => WindowClosing());
                }

                return _windowClosingCommand;
            }
        }

        public bool ShowLivestream
        {
            get { return _showLivestream; }
            set
            {
                _showLivestream = value;
                ToggleLivestream(_showLivestream);
            }
        }

        public bool CaptureImageButtonEnabled { get; set; } = true;
        public int CurrentFramerate { get; set; }

        public int PreferredFramerate
        {
            get { return _preferredFramerate; }
            set
            {

                capture.Set(Emgu.CV.CvEnum.CapProp.Fps, value);
                CurrentFramerate = (int) capture.Get(Emgu.CV.CvEnum.CapProp.Fps);
                _preferredFramerate = value;
            }
        }

        private void CaptureImage()
        {
                Mat frame = capture.QueryFrame();

                if (frame == null)
                    return;

                Bitmap bmp = frame.ToBitmap();
                ImageSource = bmp.ToBitmapSource();
        }

        private void Capture_ImageGrabbed(object? sender, EventArgs e)
        {
            Mat frame = new Mat();

            if (!capture.Retrieve(frame))
                return;

            Bitmap bmp = frame.ToBitmap();
            ImageSource.Dispatcher.Invoke(() => ImageSource = bmp.ToBitmapSource());
        }

        private void WindowClosing()
        {
            capture.Stop();
        }

        private void ToggleLivestream(bool showLivestream)
        {
            CaptureImageButtonEnabled = !showLivestream;

            if (showLivestream)
            {
                capture.Start();
                CurrentFramerate = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);
            }
            else
            {
                capture.Stop();
            }
        }
    }
}
