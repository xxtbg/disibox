using System;

namespace Disibox.Data
{
    public interface IStorage
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// 
        /// </summary>
        void Clear();

        /// <summary>
        /// 
        /// </summary>
        void Delete();

        /// <summary>
        /// 
        /// </summary>
        bool Exists();
    }
}