using System.ServiceModel;

// Questa interfaccia serve per poter fare la publicazione del servizio WCF
namespace Reverse {
    [ServiceContract]
    public interface IReverse {
        [OperationContract]
        string ReverseString(string stringToReverse);
    }
}
