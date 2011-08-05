using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disibox.Processing
{
    public class MD5Calculator : BaseProcessor<string>
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

        public override string ProcessFile()
        {
            throw new NotImplementedException();
        }
    }
}
