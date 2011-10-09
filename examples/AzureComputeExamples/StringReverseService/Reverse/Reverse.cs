using System.Linq;
using System.ServiceModel;

namespace Reverse {
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)] //per accettare conessioni non solo HTTP
    public class Reverse : IReverse {
        public string ReverseString(string stringToReverse) {
            var temp = new char[stringToReverse.Count()];

            for (var i = 0; i < stringToReverse.Count(); i++)
                temp[stringToReverse.Count() - 1 - i] = stringToReverse[i];


            return new string(temp);
        }
    }
}
