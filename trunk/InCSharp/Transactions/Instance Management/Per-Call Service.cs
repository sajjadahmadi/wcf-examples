using System;
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

    	readonly NetNamedPipeBinding binding = new NetNamedPipeBinding();

        [TestMethod]
        public void PerCallTransactionService()
        {
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (var host = new ServiceHost(typeof(PerCallService)))
            using (var proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint(typeof(IInstanceIdGetter), binding, address);
                host.Open();
                var first = proxy.GetInstanceId();
                var second = proxy.GetInstanceId();
                Assert.AreNotEqual(second, first, "Expected a different instance.");
            }
        }

    }
}
