using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    /// <summary>
    /// Summary description for PerCallServiceTests
    /// </summary>
    [TestClass]
    public class PerCallServiceTests
    {
        #region  Service
        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = true)] // Default
        class PerCallService : InstanceIdSetter, IInstanceIdGetter
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
        public void PerCallTransactionService()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<PerCallService> host = new ServiceHost<PerCallService>())
            using (ServiceClient proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint<IInstanceIdGetter>(binding, address);
                host.Open();
                Guid first = proxy.GetInstanceId();
                Guid second = proxy.GetInstanceId();
                Assert.AreNotEqual(second, first, "Expected a different instance.");
            }
        }

    }
}
