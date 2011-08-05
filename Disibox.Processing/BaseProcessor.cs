using System.Collections.Generic;

namespace Disibox.Processing
{
    abstract class BaseProcessor<TResult> : IProcessor<TResult>
    {
        protected BaseProcessor()
        {
            ProcessableTypes = new List<string>();
        }

        public abstract string Name { get; }
        
        public abstract string BriefDescription { get; }
        
        public abstract string LongDescription { get; }

        public IEnumerable<string> ProcessableTypes { get; private set; }
        
        public abstract TResult ProcessFile();
    }
}
