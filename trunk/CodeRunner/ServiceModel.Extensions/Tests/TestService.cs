
namespace System.ServiceModel.Test
{
    [ServiceContract]
    public interface ITestContract
    {
        [OperationContract]
        string MyOperation();
    }

    public class TestService : ITestContract
    {
        public string MyOperation()
        {
            return "MyResult";
        }
    }
}