﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Disibox.Processing.Tools
{
    public class ColorInverter : BaseTool
    {
        private const string BmpContentType = "image/bmp";
        private const string JpegContentType = "image/jpeg";
        private const string PngContentType = "image/png";

        public ColorInverter()
        {
            ProcessableTypes.Add(BmpContentType);
            ProcessableTypes.Add(JpegContentType);
            ProcessableTypes.Add(PngContentType);
        }

        public override string Name
        {
            get { return "Color inverter"; }
        }

        public override string BriefDescription
        {
            get { return "Inverts image colors!"; }
        }

        public override string LongDescription
        {
            get { throw new NotImplementedException(); }
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var bitmap = (Bitmap) (new Bitmap(file)).Clone();

            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(area, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var stride = data.Stride;

            var nWidth = bitmap.Width * 3;
            var nHeight = bitmap.Height;
            var nOffset = stride - bitmap.Width * 3;
            
            var scan0 = data.Scan0;
            unsafe
            {
                var p = (byte*)(void*)scan0;
                
                for (var y = 0; y < nHeight; ++y)
                {
                    for (var x = 0; x < nWidth; ++x)
                    {
                        *p = (byte)(255 - *p);
                        ++p;
                    }
                    p += nOffset;
                }
            }
            
            bitmap.UnlockBits(data);

            var invertedStream = new MemoryStream();
            var invertedFormat = GetFormatFromContentType(fileContentType);
            bitmap.Save(invertedStream, invertedFormat);
            return new ProcessingOutput(invertedStream, fileContentType);
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