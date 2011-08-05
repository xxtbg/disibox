using System.Collections.Generic;

namespace Disibox.Processing
{
    public interface IProcessor<out TResult>
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
        IEnumerable<string> GetProcessableTypes();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TResult ProcessFile();
    }
}
