using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class Transcations
    {
        static NetNamedPipeBinding txBinding;
        static NetNamedPipeBinding noTxBinding;

        static ServiceHost<TxService> txHost;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

        #region Additional test attributes
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            txBinding = new NetNamedPipeBinding();
            txBinding.TransactionFlow = true;
            txBinding.TransactionProtocol = TransactionProtocol.OleTransactions;

            noTxBinding = new NetNamedPipeBinding();
            noTxBinding.TransactionFlow = false;

            txHost = new ServiceHost<TxService>();
            txHost.AddServiceEndpoint<IServiceTxContract>(txBinding, address);
            txHost.IncludeExceptionDetailInFaults = true;
            txHost.Open();
        }
        [ClassCleanup()]
        public static void MyClassCleanup()
        { txHost.Close(); }
        #endregion


        class ServiceTxClient : ClientBase<IServiceTxContract>, IServiceTxContract
        {
            public ServiceTxClient(Binding binding, string remoteAddress)
                : base(binding, new EndpointAddress(remoteAddress)) { }

            #region IServiceTxContract Members

            public TransactionInfo ClientTxNotAllowedMethod()
            { return Channel.ClientTxNotAllowedMethod(); }

            public TransactionInfo ClientTxAllowedMethod()
            { return Channel.ClientTxAllowedMethod(); }

            #endregion

        }

        class ClientTxClient : ClientBase<IClientTxContract>, IClientTxContract
        {
            public ClientTxClient(Binding binding, string remoteAddress)
                : base(binding, new EndpointAddress(remoteAddress)) { }

            #region IClientTxContract Members

            public TransactionInfo ClientTxAllowedMethod()
            { return Channel.ClientTxAllowedMethod(); }

            public TransactionInfo ClientTxMandatoryMethod()
            { return Channel.ClientTxMandatoryMethod(); }

            #endregion

        }



        [TestMethod]
        public void ServiceOnlyTranscationMode()
        {
            ServiceTxClient proxy = new ServiceTxClient(noTxBinding, address);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.ClientTxNotAllowedMethod();
                Assert.IsTrue(info.HasTransaction);
                Assert.IsTrue(info.DistributedIdentifier == Guid.Empty);
                Assert.IsNotNull(info.LocalIdentifier);
            }
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.ClientTxAllowedMethod();
                Assert.IsTrue(info.HasTransaction);
                Assert.IsTrue(info.DistributedIdentifier == Guid.Empty);
                Assert.IsNotNull(info.LocalIdentifier);
            }
            proxy.Close();
        }

        [TestMethod]
        public void ClientServiceTranscationMode()
        {
            ServiceTxClient proxy = new ServiceTxClient(txBinding, address);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.ClientTxNotAllowedMethod();
                Assert.IsTrue(info.HasTransaction);
                Assert.IsTrue(info.DistributedIdentifier == Guid.Empty);
            }
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.ClientTxAllowedMethod();
                Assert.IsTrue(info.HasTransaction);
                Assert.IsTrue(info.DistributedIdentifier != Guid.Empty);
            }
            proxy.Close();
        }
    }
}
