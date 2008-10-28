using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class FaultExceptionTests
    {
        #region Additional test attributes
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/";
        static ServiceHost<MyService> host;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.Open();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(FaultException<FaultType>))]
        public void TypedFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            client.Open();
            client.ThrowTypedFault();
            client.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void UntypedFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            client.Open();
            client.ThrowUntypedFault();
            client.Close();
        }

    }
}
