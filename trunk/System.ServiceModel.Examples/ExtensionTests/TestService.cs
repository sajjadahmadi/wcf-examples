
namespace System.ServiceModel.Examples
{
    [ServiceContract]
    interface ITestContract
    {
        [OperationContract]
        string MyOperation();
    }

    class TestService : ITestContract
    {
        public string MyOperation()
        {
            return "MyResult";
        }
    }
}