using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class Transcations
    {
        static NetNamedPipeBinding txBinding;
        static NetNamedPipeBinding noTxBinding;

        static ServiceHost<TxService> txHost;
        static string address = "net.pipe://localhost/TxService";

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
            txHost.AddServiceEndpoint<IClientTxNotAllowed>(noTxBinding, address);
            txHost.Open();
        }
        [ClassCleanup()]
        public static void MyClassCleanup()
        { txHost.Close(); }
        #endregion


        class ClientTxNotAllowedClient : ClientBase<IClientTxNotAllowed>, IClientTxNotAllowed
        {
            public ClientTxNotAllowedClient(Binding binding, string remoteAddress)
                : base(binding, new EndpointAddress(remoteAddress)) { }

            public void MyMethod()
            { Channel.MyMethod(); }
        }


        [TestMethod]
        public void ServiceOnlyTranscationMode()
        {
            ClientTxNotAllowedClient client = new ClientTxNotAllowedClient(noTxBinding, address);
            client.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                client.MyMethod();
            }
            client.Close();
        }
    }
}
