using System.IO;
using Disibox.Utils;

namespace Disibox.Processing.Tools
{
    public class SHA256Calculator : BaseTool
    {
        public SHA256Calculator()
            : base("SHA256 calculator", "Calculates SHA256 hash of given file.", "")
        {
            
        }

        public override ProcessingOutput ProcessFile(Stream file, string fileContentType)
        {
            var sha256 = Hash.ComputeSHA256(file);
            var streamedSHA256 = new MemoryStream(Common.StringToByteArray(sha256));
            return new ProcessingOutput(streamedSHA256, "text/plain");
        }
    }
}
