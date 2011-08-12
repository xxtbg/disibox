using System.IO;
using Disibox.Utils;

namespace Disibox.Processing.Tools
{
    public class MD5Calculator : BaseTool
    {
        public MD5Calculator()
            : base("MD5 calculator", "Calculates MD5 hash of given file.", "")
        {
            // Empty
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var md5 = Hash.ComputeMD5(file);
            var streamedMD5 = new MemoryStream(Common.StringToByteArray(md5));
            return new ProcessingOutput(streamedMD5, "text/plain");
        }
    }
}
