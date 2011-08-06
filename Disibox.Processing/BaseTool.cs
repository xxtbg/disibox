using System.Collections.Generic;

namespace Disibox.Processing
{
    public abstract class BaseTool : ITool
    {
        protected BaseTool()
        {
            ProcessableTypes = new List<string>();
        }

        public abstract string Name { get; }
        
        public abstract string BriefDescription { get; }
        
        public abstract string LongDescription { get; }

        public IEnumerable<string> ProcessableTypes { get; private set; }
        
        public abstract object ProcessFile();
    }
}
