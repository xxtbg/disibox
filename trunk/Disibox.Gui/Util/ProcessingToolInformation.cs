using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disibox.Gui.Util
{
    public class ProcessingToolInformation
    {
        public ProcessingToolInformation(string name, string briefDescription, string longDescription)
        {
            Name = name;
            BriefDescription = briefDescription;
            LongDescription = longDescription;
        }
        public string Name { get; private set; }
        public string BriefDescription { get; private set; }
        public string LongDescription { get; private set; }
        
    }
}
