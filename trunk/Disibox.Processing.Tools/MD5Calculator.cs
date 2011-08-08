using System;
using System.IO;
using Disibox.Utils;

namespace Disibox.Processing.Tools
{
    public class MD5Calculator : BaseTool
    {
        public override string Name
        {
            get { return "MD5 Calculator"; }
        }

        public override string BriefDescription
        {
            get { return "Calculates MD5 hash of given file."; }
        }

        public override string LongDescription
        {
            get { throw new NotImplementedException(); }
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var md5 = Hash.ComputeMD5(file);
            var streamedMD5 = new MemoryStream(Common.StringToByteArray(md5));
            return new ProcessingOutput(streamedMD5, "text/plain");
        }
    }
}
