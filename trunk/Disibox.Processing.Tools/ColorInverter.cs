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
using Color = System.Drawing.Color;

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
            var bitmap = new Bitmap(file, true);
            var inverted = SlowInvertImage(bitmap);
            var invertedStream = new MemoryStream();
            inverted.Save(invertedStream, format);
            return new ProcessingOutput(invertedStream, fileContentType);
        }

        private static Image SlowInvertImage(Image originalImg)
        {
            var invertedBmp = new Bitmap(originalImg.Width, originalImg.Height);
            using (var originalBmp = new Bitmap(originalImg))
            {
                for (var x = 0; x < originalBmp.Width; x++)
                {
                    for (var y = 0; y < originalBmp.Height; y++)
                    {
                        var color = originalBmp.GetPixel(x, y);
                        color = Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
                        invertedBmp.SetPixel(x, y, color);
                    }
                }
            }
            return invertedBmp;
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