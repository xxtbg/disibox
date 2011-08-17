using System;

namespace Disibox.Utils
{
    public static class Require
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull(object arg, string argName)
        {
            if (arg != null) return;
            throw new ArgumentNullException(argName);
        }
    }
}
