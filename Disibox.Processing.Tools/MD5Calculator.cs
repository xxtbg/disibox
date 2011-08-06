using System;

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

        public override object ProcessFile()
        {
            throw new NotImplementedException();
        }
    }
}
