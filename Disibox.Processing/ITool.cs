using System.Collections.Generic;
using System.IO;

namespace Disibox.Processing
{
    public interface ITool
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        string BriefDescription { get; }

        /// <summary>
        /// 
        /// </summary>
        string LongDescription { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IList<string> ProcessableTypes { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ProcessingOutput ProcessFile(Stream file, string fileContentType);
    }
}
