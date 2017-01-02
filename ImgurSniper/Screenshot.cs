﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgurSniper {
    class Screenshot {

        public static ImageSource getScreenshot(Rectangle coordinates, bool method) {
            int left = coordinates.Left;
            int top = coordinates.Top;
            int width = coordinates.Width;
            int height = coordinates.Height;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //TODO: Choose faster one (Probably else Block)
            if(method) {
                //Thanks http://stackoverflow.com/users/214375/marcel-gheorghita !

                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(left, top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);


                byte[] byteImage = ImageToByte(bmp);

                bmp.Dispose();

                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(byteImage);
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();

                ImageSource imgSrc = biImg as ImageSource;

                sw.Stop();
                MessageBox.Show(sw.ElapsedMilliseconds + method.ToString());

                return imgSrc;
            } else {
                //Thanks http://stackoverflow.com/users/183367/julien-lebosquain !

                using(var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                    using(var bmpGraphics = Graphics.FromImage(screenBmp)) {
                        bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));

                        IntPtr hBitmap = screenBmp.GetHbitmap();

                        BitmapSource ret = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                        DeleteObject(hBitmap);

                        sw.Stop();
                        MessageBox.Show(sw.ElapsedMilliseconds + method.ToString());

                        return ret;
                    }
                }
            }
        }


        public static byte[] ImageToByte(Image img) {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }


        public static Image MediaImageToDrawingImage(ImageSource image) {
            MemoryStream ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return Image.FromStream(ms);
        }

        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
    }
}
