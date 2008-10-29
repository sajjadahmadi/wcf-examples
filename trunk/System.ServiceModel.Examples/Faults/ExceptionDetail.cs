using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class FaultExceptionDetail
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
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.IncludeExceptionDetailInFaults = true;
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
        [ExpectedException(typeof(FaultException<ExceptionDetail>))]
        public void ExceptionDetail()
        {
            MyContractClient client = new MyContractClient(binding, address);
            client.Open();
            try
            {                client.ThrowClrException();            }
            catch (FaultException<ExceptionDetail> ex)
            {
                Assert.AreEqual("System.NotImplementedException", ex.Detail.Type);
                throw;
            }
            client.Close();
        }
    }
}
