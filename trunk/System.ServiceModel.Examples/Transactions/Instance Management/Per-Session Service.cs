using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    /// <summary>
    /// Summary description for PerSessionServiceTests
    /// </summary>
    [TestClass]
    public class PerSessionServiceTests
    {
        #region  Service
        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = false)]
        class PerSessionService : InstanceIdSetter, IInstanceIdGetter
        {
            [OperationBehavior(TransactionScopeRequired = true)]
            public Guid GetInstanceId()
            {
                return base.instanceId;
            }
        }
        #endregion

        NetNamedPipeBinding binding = new NetNamedPipeBinding();

        [TestMethod]
        public void PerSessionTransactionService()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<PerSessionService> host = new ServiceHost<PerSessionService>())
            using (ServiceClient proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint<IInstanceIdGetter>(binding, address);
                host.Open();
                Guid first = proxy.GetInstanceId();
                Guid second = proxy.GetInstanceId();
                Assert.AreEqual(second, first);
            }
        }
    }
}
