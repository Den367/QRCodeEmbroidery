using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace EmbroideryFile
{
    public static class BitmapExtension
    {
        public static ImageFormat GetImageFormat(this System.Drawing.Image img)
        {
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                return System.Drawing.Imaging.ImageFormat.Png;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                return ImageFormat.Jpeg;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                return ImageFormat.Bmp;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf))
                return System.Drawing.Imaging.ImageFormat.Emf;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif))
                return System.Drawing.Imaging.ImageFormat.Exif;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                return System.Drawing.Imaging.ImageFormat.Gif;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                return System.Drawing.Imaging.ImageFormat.Icon;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
                return System.Drawing.Imaging.ImageFormat.MemoryBmp;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                return System.Drawing.Imaging.ImageFormat.Tiff;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Wmf))
                return System.Drawing.Imaging.ImageFormat.Wmf;
            else return null;
        }

        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static void SaveToFile(this Bitmap bitmap, string filename, ImageFormat format)
        {
            bitmap.Save(filename, format);
        }

        public static Bitmap ResizeImage(this Bitmap image, int width, int height)
        {
            Bitmap oldImage = image;
            //a holder for the result
            Bitmap result = new Bitmap(width, height);
            // set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }
            oldImage.Dispose();
            image = result;
            return result;

            //return the resulting bitmap

        }

        static public Color ToColor(this string argb)
        {
            return Color.FromArgb(Int32.Parse(argb, NumberStyles.HexNumber));
        }

    }
}