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

        public override object ProcessFile(Stream file, string fileContentType)
        {
            return Hash.ComputeSHA256(file);
        }
    }
}
