using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class CallbackFaults
    {
        #region Additional test attributes
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract2>(binding, address);
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
        [ExpectedException(typeof(FaultException))]
        public void CallbackFault()
        {
            MyContractCallback callback = new MyContractCallback();

            MyClientWithCallback client = new MyClientWithCallback(callback, binding, address);
            client.Open();
            client.CallbackAndCatchFault();
            client.Close();
        }
    }
}
