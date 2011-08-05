using System;

namespace Disibox.Data.Exceptions
{
    public class SpecialUserRequiredException : Exception
    {
        public SpecialUserRequiredException(UserType userType) : base(userType.ToString())
        {
            // Empty
        }
    }
}
