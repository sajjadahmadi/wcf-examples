using System.ServiceModel;

namespace AdministrativeConfiguration {

    [ServiceContract]
    interface IMyContract {

        [OperationContract]
        string MyOperation();
    }

    class MyService : IMyContract {

        public string MyOperation() {
            return "MyResult";
        }
    }
}
