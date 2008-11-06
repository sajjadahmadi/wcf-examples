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
    /// <summary>
    /// TransactionFlow - From the client
    /// TransactionScopeRequire - Service requires transaction
    /// Scope   Binding	Flow	Result
    /// FALSE	FALSE	FALSE	Method executes without a transaction.
    /// TRUE	FALSE	FALSE	Method creates and executes within a new transaction.
    ///         FALSE	TRUE	A SOAP fault is returned for the transaction header.
    /// FALSE	TRUE	TRUE	Method executes without a transaction.
    /// TRUE	TRUE	TRUE	Method executes under the flowed transaction.
    /// </summary>
    [TestClass]
    public class Transcations
    {
        static NetNamedPipeBinding txBinding;
        static NetNamedPipeBinding noTxBinding;

        static ServiceHost<MyService> host;
        static string noFlowAddress = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static string txFlowAddress = "net.pipe://localhost/" + Guid.NewGuid().ToString();

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            txBinding = new NetNamedPipeBinding();
            txBinding.TransactionFlow = true;
            txBinding.TransactionProtocol = TransactionProtocol.OleTransactions;

            noTxBinding = new NetNamedPipeBinding();
            noTxBinding.TransactionFlow = false;

            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<ITxFlow>(txBinding, txFlowAddress);
            host.AddServiceEndpoint<INoFlow>(txBinding, noFlowAddress);
            host.IncludeExceptionDetailInFaults = true;
            host.Open();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }


        /// <summary>
        /// ScopeNotRequired() 
        /// Transaction Mode:  NONE
        /// </summary>
        [TestMethod]
        public void ScopeNotRequired()
        {
            NoFlowClient proxy = new NoFlowClient(noTxBinding, noFlowAddress);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.ScopeNotRequired();
                Assert.IsFalse(info.HasAmbientTransaction, "Expected no ambient transaction.");
            }
            proxy.Close();
        }

        /// <summary>
        /// ScopeRequired_FlowUnspecified() 
        /// Transaction Mode: Service
        /// Client's transaction is ignored.
        /// Service creates new transaction.
        /// Same as Flow NotAllowed
        /// </summary>
        [TestMethod]
        public void ScopeRequired_FlowUnspecified()
        {
            NoFlowClient proxy = new NoFlowClient(noTxBinding, noFlowAddress);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.FlowUnspecified();
                Assert.IsTrue(info.UsingServiceSideTransaction, "Expected service-side transaction");
                Assert.IsFalse(info.UsingClientSideTransaction);
            }
            proxy.Close();
        }

        /// <summary>
        /// ScopeRequired_FlowNotAllowed()
        /// Transaction Mode: Service
        /// Client's transaction is ignored.
        /// Service creates new transaction.
        /// Same as Flow NotSpecified
        /// </summary>
        [TestMethod]
        public void ScopeRequired_FlowNotAllowed()
        {
            NoFlowClient proxy = new NoFlowClient(noTxBinding, noFlowAddress);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.FlowNotAllowed();
                Assert.IsTrue(info.UsingServiceSideTransaction, "Expected service-side transaction");
                Assert.IsFalse(info.UsingClientSideTransaction);
            }
            proxy.Close();
        }

        /// <summary>
        /// ScopeRequired_FlowAllowed
        /// Transaction Mode: Client/Serivce 
        /// Uses the client's transaction if one exists, 
        /// otherwise service creates a new transaction
        /// </summary>
        [TestMethod]
        public void ScopeRequired_FlowAllowed()
        {
            TxFlowClient proxy = new TxFlowClient(txBinding, txFlowAddress);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.FlowAllowed();
                Assert.IsFalse(info.UsingServiceSideTransaction, "Expected client-side transaction.");
                Assert.IsTrue(info.UsingClientSideTransaction);
            }
            {
                TransactionInfo info = proxy.FlowAllowed();
                Assert.IsTrue(info.UsingServiceSideTransaction, "Expected service-side transaction.");
                Assert.IsFalse(info.UsingClientSideTransaction);
            }
            proxy.Close();
        }

        /// <summary>
        /// ScopeRequired_FlowMandatory
        /// Transaction Mode: Client 
        /// Requires the client to flow a transaction.
        /// </summary>
        [TestMethod]
        public void ScopeRequired_FlowMandatory()
        {
            TxFlowClient proxy = new TxFlowClient(txBinding, txFlowAddress);
            proxy.Open();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                TransactionInfo info = proxy.FlowMandatory();
                Assert.IsTrue(info.UsingClientSideTransaction, "Expected client-side transaction.");
                Assert.IsFalse(info.UsingServiceSideTransaction);
            }
            proxy.Close();
        }

        /// <summary>
        /// ScopeRequired_FlowMandatory_ProtocolException
        /// Transaction Mode: Client 
        /// Requires the client to flow a transaction.
        /// Throws ProtocolException when transaction is not flowed.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ProtocolException), 
            "The service operation requires a transaction to be flowed.")]
        public void ScopeRequired_FlowMandatory_ProtocolException()
        {
            TxFlowClient proxy = new TxFlowClient(txBinding, txFlowAddress);
            proxy.Open();
            TransactionInfo info = proxy.FlowMandatory();
            proxy.Close();
        }
    }
}
