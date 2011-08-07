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
        IEnumerable<string> ProcessableTypes { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object ProcessFile(Stream file, string fileContentType);
    }
}
