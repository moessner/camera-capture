using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Extensions
{
    public static class ImageExtensions
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);


        // https://stackoverflow.com/a/16597958
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            using (Bitmap source = bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
