using System;
using System.IO;
using Disibox.Utils;

namespace Disibox.Processing.Tools
{
    public class SHA256Calculator : BaseTool
    {
        public override string Name
        {
            get { return "SHA256 Calculator"; }
        }

        public override string BriefDescription
        {
            get { return "Calculates SHA256 hash of given file."; }
        }

        public override string LongDescription
        {
            get { throw new NotImplementedException(); }
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var sha256 = Hash.ComputeMD5(file);
            var streamedSHA256 = new MemoryStream(Common.StringToByteArray(sha256));
            return new ProcessingOutput(streamedSHA256, "text/plain");
        }
    }
}
