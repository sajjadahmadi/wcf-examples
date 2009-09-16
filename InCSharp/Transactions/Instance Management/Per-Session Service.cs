using System;
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

    	readonly NetNamedPipeBinding binding = new NetNamedPipeBinding();

        [TestMethod]
        public void PerSessionTransactionService()
        {
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (var host = new ServiceHost(typeof(PerSessionService)))
            using (var proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint(typeof(IInstanceIdGetter), binding, address);
                host.Open();
                var first = proxy.GetInstanceId();
                var second = proxy.GetInstanceId();
                Assert.AreEqual(second, first);
            }
        }
    }
}
