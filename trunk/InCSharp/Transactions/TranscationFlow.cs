using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
	#region Service Contracts
	[DataContract]
	struct TransactionInfo
	{
	    [DataMember]
		public Guid DistributedIdentifier { get; set; }

		[DataMember]
		public string LocalIdentifier { get; set; }

		[DataMember]
		public Guid InstanceIdentifier { get; set; }
	}

	[ServiceContract]
	interface INoFlow
	{
		[OperationContract]
		TransactionInfo NoScope();

		[OperationContract]
		TransactionInfo ScopeWithFlowUnspecified();

		[OperationContract]
		[TransactionFlow(TransactionFlowOption.NotAllowed)]
		TransactionInfo ScopeWithFlowNotAllowed();
	}

	[ServiceContract]
	interface ITxFlow
	{
		[OperationContract]
		[TransactionFlow(TransactionFlowOption.Allowed)]
		TransactionInfo ScopeWithFlowAllowed();

		[OperationContract]
		[TransactionFlow(TransactionFlowOption.Mandatory)]
		TransactionInfo ScopeWithFlowMandatory();
	}

	#endregion

	#region Service
	// Use the client's isolation level if one exists, otherwise Serializable
	// Serializable - The highest degree of isolation
	[ServiceBehavior(TransactionIsolationLevel = IsolationLevel.Unspecified, IncludeExceptionDetailInFaults = true)]
	//[BindingRequirementAttribute(TransactionFlowRequired = true)]
	class TransactionFlowService : INoFlow, ITxFlow
	{
		readonly Guid instanceId = Guid.NewGuid();

		public TransactionInfo GetTransactionInfo()
		{
			var info = new TransactionInfo();
			var tx = Transaction.Current;
			if (tx != null)
			{
				info.DistributedIdentifier = tx.TransactionInformation.DistributedIdentifier;
				info.LocalIdentifier = tx.TransactionInformation.LocalIdentifier;
				info.InstanceIdentifier = instanceId;
			}
			return info;
		}

		#region INoFlow Members

		[OperationBehavior(TransactionScopeRequired = false)]
		TransactionInfo INoFlow.NoScope()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo INoFlow.ScopeWithFlowUnspecified()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo INoFlow.ScopeWithFlowNotAllowed()
		{
			return GetTransactionInfo();
		}

		#endregion

		#region ITxFlow Members

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo ITxFlow.ScopeWithFlowAllowed()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo ITxFlow.ScopeWithFlowMandatory()
		{
			return GetTransactionInfo();
		}

		#endregion
	}
	#endregion

	#region Clients
	class NoFlowClient : ClientBase<INoFlow>, INoFlow
	{
		public NoFlowClient(Binding binding, string remoteAddress)
			: base(binding, new EndpointAddress(remoteAddress)) { }

		#region IServiceTxContract Members

		public TransactionInfo NoScope()
		{ return Channel.NoScope(); }

		public TransactionInfo ScopeWithFlowUnspecified()
		{ return Channel.ScopeWithFlowUnspecified(); }

		public TransactionInfo ScopeWithFlowNotAllowed()
		{ return Channel.ScopeWithFlowNotAllowed(); }

		#endregion
	}

	class TxFlowClient : ClientBase<ITxFlow>, ITxFlow
	{
		public TxFlowClient(Binding binding, string remoteAddress)
			: base(binding, new EndpointAddress(remoteAddress)) { }

		#region IClientTxContract Members

		public TransactionInfo ScopeWithFlowAllowed()
		{ return Channel.ScopeWithFlowAllowed(); }

		public TransactionInfo ScopeWithFlowMandatory()
		{ return Channel.ScopeWithFlowMandatory(); }

		#endregion
	}

	#endregion

	/// <summary>
	/// TransactionFlow - Transaction flows from the client
	/// TransactionScopeRequire - Service requires transaction
	/// Scope   Binding  Flow    Result
	/// FALSE   FALSE    FALSE   Method executes without a transaction.
	/// FALSE   TRUE     TRUE    Method executes without a transaction.
	/// Any	   FALSE    TRUE    A SOAP fault is returned for the transaction header.
	/// TRUE    FALSE    FALSE   Method creates and executes within a new transaction.
	/// TRUE    TRUE     TRUE    Method executes under the flowed transaction.
	/// </summary>
	[TestClass]
	public class TranscationFlowTests
	{
		static NetNamedPipeBinding txFlowBinding;
		static NetNamedPipeBinding noFlowBinding;

		static ServiceHost host;
		static readonly string noFlowAddress = "net.pipe://localhost/" + Guid.NewGuid();
		static readonly string txFlowAddress = "net.pipe://localhost/" + Guid.NewGuid();

		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			txFlowBinding = new NetNamedPipeBinding
								{
									TransactionFlow = true,
									TransactionProtocol = TransactionProtocol.OleTransactions
								};

			noFlowBinding = new NetNamedPipeBinding { TransactionFlow = false };

			host = new ServiceHost(typeof(TransactionFlowService));
			host.AddServiceEndpoint(typeof(ITxFlow), txFlowBinding, txFlowAddress);
			host.AddServiceEndpoint(typeof(INoFlow), txFlowBinding, noFlowAddress);
			host.Open();
		}

		[ClassCleanup]
		public static void MyClassCleanup()
		{
			using (host)
			{
			}
		}


		/// <summary>
		/// ScopeNotRequired() 
		/// Transaction Mode:  NONE
		/// </summary>
		[TestMethod]
		public void ScopeNotRequired()
		{
			var proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				var info = proxy.NoScope();
				Assert.IsNull(info.LocalIdentifier,  "Service should not have a transaction.");
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
			var proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				var info = proxy.ScopeWithFlowUnspecified();
				Assert.IsTrue(info.DistributedIdentifier == Guid.Empty, "Binding should not allow ambient transaction to flow.");
				Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");
			}
			proxy.Close();
		}

		/// <summary>
		/// ScopeRequired_NoTransactionScopeOnClient() 
		/// Transaction Mode: Service
		/// Service creates a new transaction that client knows nothing about.
		/// </summary>
		[TestMethod]
		public void ScopeRequired_NoTransactionScopeOnClient()
		{
			var proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			var info = proxy.ScopeWithFlowUnspecified();
			Assert.IsTrue(info.DistributedIdentifier == Guid.Empty, "No ambient transaction to flow.");
            Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");
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
			var proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				var info = proxy.ScopeWithFlowNotAllowed();

				Assert.IsTrue(info.DistributedIdentifier == Guid.Empty, "Flow not allowed should prevent ambient transaction from flowing.");
                Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");
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
			var proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
			proxy.Open();
			using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				var info = proxy.ScopeWithFlowAllowed();
				Assert.IsTrue(info.DistributedIdentifier != Guid.Empty, "Client transaction should flow.");
                Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");

			}
			{
				var info = proxy.ScopeWithFlowAllowed();
				Assert.IsTrue(info.DistributedIdentifier == Guid.Empty, "No ambient transaction to flow.");
                Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");
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
			var proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
			proxy.Open();
			using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				var info = proxy.ScopeWithFlowMandatory();
				Assert.IsTrue(info.DistributedIdentifier != Guid.Empty, "Client transaction should flow.");
                Assert.IsNotNull(info.LocalIdentifier, "Service should have a transaction.");
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
			var proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
			proxy.Open();
			var info = proxy.ScopeWithFlowMandatory();
			proxy.Close();
		}
	}
}
