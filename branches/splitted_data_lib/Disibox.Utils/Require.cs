using System;
using Disibox.Utils.Exceptions;

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

        /// <summary>
        /// Checks if given string is a valid email address.
        /// </summary>
        /// <param name="email"></param>
        /// <exception cref="InvalidEmailException"></exception>
        public static void ValidEmail(string email)
        {
            
        }
    }
}
