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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Threading;
using System.IO;

namespace view_models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ICommand _captureImageCommand;
        private ICommand _windowClosingCommand;
        private ICommand _copyImageCommand;
        private ICommand _saveImageCommand;
        private ICommand _discardImageCommand;
        private bool _showLivestream;
        private int _preferredFramerate = 60;
        private VideoCapture capture;

        public MainViewModel()
        {
            capture = new VideoCapture();
            capture.ImageGrabbed += Capture_ImageGrabbed;

            ShowLivestream = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource ImageSource { get; set; } = new BitmapImage();

        public ObservableCollection<System.Windows.Controls.Image> CapturedImages { get; set; } = new ObservableCollection<System.Windows.Controls.Image>();

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
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                {
                    _copyImageCommand = new RelayCommand(() => CopySelectedImage());
                }

                return _copyImageCommand;
            }
        }

        public ICommand SaveImageCommand
        {
            get
            {
                if (_saveImageCommand == null)
                {
                    _saveImageCommand = new RelayCommand(() => SaveSelectedImage());
                }

                return _saveImageCommand;
            }
        }
        
        public ICommand DiscardImageCommand
        {
            get
            {
                if (_discardImageCommand == null)
                {
                    _discardImageCommand = new RelayCommand(() => DiscardSelectedImage());
                }

                return _discardImageCommand;
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
        private System.Windows.Controls.Image _selectedImage;

        public System.Windows.Controls.Image SelectedImage
        {
            get { return _selectedImage; }
            set 
            { 
                _selectedImage = value;
                ImageSource = value != null ? value.Source : new BitmapImage();
            }
        }


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
            BitmapSource source = bmp.ToBitmapSource();
            ImageSource = source;

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = source;
            CapturedImages.Add(image);
        }

        private void Capture_ImageGrabbed(object? sender, EventArgs e)
        {
            Mat frame = new Mat();

            if (!capture.Retrieve(frame))
                return;

            Bitmap bmp = DetectFaces(frame);
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

        private void SaveSelectedImage()
        {
            SaveFileDialog sfd = new SaveFileDialog();

            BitmapSource source = (BitmapSource)SelectedImage.Source;

            if (!(bool)sfd.ShowDialog())
                return;

            using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fileStream);
            }
        }

        private void CopySelectedImage()
        {
            BitmapSource source = (BitmapSource)SelectedImage.Source;
            Bitmap bitmap = source.ConvertToBitmap();
            Clipboard.SetDataObject(bitmap);
        }

        private Bitmap DetectFaces(Mat mat)
        {
            CascadeClassifier cascadeClassifier = new CascadeClassifier(Path.GetFullPath("./pretrained_models/haarcascade_frontalface_alt2.xml"));
            Rectangle[] rectFaces = cascadeClassifier.DetectMultiScale(mat);

            Bitmap bitmap = mat.ToBitmap();

            foreach (Rectangle rect in rectFaces)
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawRectangle(Pens.Red, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }
           
            return bitmap;
        }

        private void DiscardSelectedImage()
        {
            CapturedImages.Remove(SelectedImage);
        }
    }
}
