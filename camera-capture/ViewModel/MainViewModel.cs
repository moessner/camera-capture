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

namespace view_models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ICommand _captureImageCommand;
        private ICommand _windowClosingCommand;
        private bool _showLivestream;
        private int _framerate = 60;
        private Timer livestreamTimer;

        public MainViewModel()
        {
            livestreamTimer = new Timer();
            livestreamTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                this.ImageSource.Dispatcher.Invoke(() => CaptureImage());
            });

            livestreamTimer.Interval = Framerate;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource ImageSource { get; set; } = new BitmapImage();
        public bool IsTestMode { get; set; }

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

        public int Framerate
        {
            get { return _framerate; }
            set
            {
                _framerate = value;
                livestreamTimer.Interval = 1000 / _framerate;
            }
        }

        private void CaptureImage()
        {
            if (IsTestMode)
                ImageSource = GenerateTestImage();
            else
            {
                VideoCapture capture = new VideoCapture();
                Mat frame = capture.QueryFrame();

                if (frame == null)
                    return;

                Bitmap bmp = frame.ToBitmap();
                ImageSource = bmp.ToBitmapSource();
            }
        }

        private void WindowClosing()
        {
            livestreamTimer.Stop();
        }


        // https://stackoverflow.com/a/21891278
        private DrawingImage GenerateTestImage()
        {
            var random = new Random();
            var pixels = new byte[256 * 256 * 4];
            random.NextBytes(pixels);

            BitmapSource source = BitmapSource.Create(256, 256, 96, 96, PixelFormats.Pbgra32, null, pixels, 256 * 4);
            var visual = new DrawingVisual();

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(source, new Rect(0, 0, 256, 256));
            }

            DrawingImage image = new DrawingImage(visual.Drawing);

            return image;
        }

        private void ToggleLivestream(bool showLivestream)
        {
            CaptureImageButtonEnabled = !showLivestream;

            if (showLivestream)
                livestreamTimer.Start();
            else
            {
                livestreamTimer.Stop();
                ImageSource = new BitmapImage();
            }
        }
    }
}
