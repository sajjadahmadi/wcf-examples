using System.ServiceModel;

namespace IISHosting {

    [ServiceContract]
    public interface IMyContract {

        [OperationContract]
        string MyOperation();
    }

    public class MyService : IMyContract {

        public string MyOperation() {
            return "MyResult";
        }
    }
}
