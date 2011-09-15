//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Disibox.Processing.Tools
{
    public class ColorInverter : BaseTool
    {
        private const string BmpContentType = "image/bmp";
        private const string JpegContentType = "image/jpeg";
        private const string PngContentType = "image/png";

        public ColorInverter()
            : base("Color inverter", "Inverts image colors!", "Inverts image colors for bmp, jpeg and png images.")
        {
            ProcessableTypes.Add(BmpContentType);
            ProcessableTypes.Add(JpegContentType);
            ProcessableTypes.Add(PngContentType);
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var format = GetFormatFromContentType(fileContentType);
            var bitmap = GetBitmapFromStream(file, format);

            /*var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = data.Stride;

            var nWidth = bitmap.Width*3;
            var nHeight = bitmap.Height;
            var nOffset = stride - bitmap.Width*3;

            var scan0 = data.Scan0;
            unsafe
            {
                var p = (byte*) (void*) scan0;

                for (var y = 0; y < nHeight; ++y)
                {
                    for (var x = 0; x < nWidth; ++x)
                    {
                        *p = (byte) (255 - *p);
                        ++p;
                    }
                    p += nOffset;
                }
            }

            bitmap.UnlockBits(data);*/

            var inverted = InvertImage(bitmap);

            var invertedStream = new MemoryStream();

            inverted.Save(invertedStream, format);
            return new ProcessingOutput(invertedStream, fileContentType);
        }

        private static Image InvertImage(Image originalImg)
        {
            Bitmap invertedBmp = null;

            using (Bitmap originalBmp = new Bitmap(originalImg))
            {
                invertedBmp = new Bitmap(originalBmp.Width, originalBmp.Height);

                for (int x = 0; x < originalBmp.Width; x++)
                {
                    for (int y = 0; y < originalBmp.Height; y++)
                    {
                        //Get the color
                        Color clr = originalBmp.GetPixel(x, y);

                        //Invert the clr
                        clr = Color.FromArgb(255 - clr.R, 255 - clr.G, 255 - clr.B);

                        //Update the color
                        invertedBmp.SetPixel(x, y, clr);
                    }
                }
            }

            return (Image)invertedBmp;
        }

        private static Image InvertImageColorMatrix(Image originalImg)
        {
            var invertedBmp = new Bitmap(originalImg.Width, originalImg.Height);

            //Setup color matrix
            var clrMatrix = new ColorMatrix(new float[][]
                                                    {
                                                    new float[] {-1, 0, 0, 0, 0},
                                                    new float[] {0, -1, 0, 0, 0},
                                                    new float[] {0, 0, -1, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {1, 1, 1, 0, 1}
                                                    });

            using (var attr = new ImageAttributes())
            {
                //Attach matrix to image attributes
                attr.SetColorMatrix(clrMatrix);

                using (Graphics g = Graphics.FromImage(invertedBmp))
                {
                    g.DrawImage(originalImg, new Rectangle(0, 0, originalImg.Width, originalImg.Height),
                                0, 0, originalImg.Width, originalImg.Height, GraphicsUnit.Pixel, attr);
                }
            }

            return invertedBmp;
        }

        private static Bitmap GetBitmapFromSource(BitmapSource source)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap 
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private static Bitmap GetBitmapFromStream(Stream input, ImageFormat format)
        {
            BitmapDecoder decoder;

            if (format == ImageFormat.Bmp)
                decoder = new BmpBitmapDecoder(input, BitmapCreateOptions.PreservePixelFormat, 
                                               BitmapCacheOption.Default);
            else if (format == ImageFormat.Png)
                decoder = new PngBitmapDecoder(input, BitmapCreateOptions.PreservePixelFormat, 
                                               BitmapCacheOption.Default);
            else // Jpeg and other formats...
                decoder = new JpegBitmapDecoder(input, BitmapCreateOptions.PreservePixelFormat,
                                                BitmapCacheOption.Default);

            var source = decoder.Frames[0];
            return GetBitmapFromSource(source);
        }

        private static ImageFormat GetFormatFromContentType(string imageContentType)
        {
            switch (imageContentType)
            {
                case BmpContentType:
                    return ImageFormat.Bmp;
                case JpegContentType:
                    return ImageFormat.Jpeg;
                case PngContentType:
                    return ImageFormat.Png;
                default:
                    throw new ArgumentException("Content type not supported.", "imageContentType");
            }
        }
    }
}