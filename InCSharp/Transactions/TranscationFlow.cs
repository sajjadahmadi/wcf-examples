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
		public bool HasAmbientTransaction
		{
			get
			{
				return (UsingServiceSideTransaction == true)
					 || (UsingClientSideTransaction == true);
			}
		}

		[DataMember]
		public bool UsingServiceSideTransaction { get; set; }

		[DataMember]
		public bool UsingClientSideTransaction { get; set; }

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
		TransactionInfo ScopeNotRequired();

		[OperationContract]
		TransactionInfo FlowUnspecified();

		[OperationContract]
		[TransactionFlow(TransactionFlowOption.NotAllowed)]
		TransactionInfo FlowNotAllowed();
	}

	[ServiceContract]
	interface ITxFlow
	{
		[OperationContract]
		[TransactionFlow(TransactionFlowOption.Allowed)]
		TransactionInfo FlowAllowed();

		[OperationContract]
		[TransactionFlow(TransactionFlowOption.Mandatory)]
		TransactionInfo FlowMandatory();
	}

	#endregion

	#region Service
	// Use the client's isolation level if one exists, otherwise Serializable
	// Serializable - The highest degree of isolation
	[ServiceBehavior(TransactionIsolationLevel = IsolationLevel.Unspecified)]
	[BindingRequirementAttribute(TransactionFlowRequired = true)]
	partial class TransactionFlowService : INoFlow, ITxFlow
	{
		Guid instanceId = Guid.NewGuid();

		public TransactionInfo GetTransactionInfo()
		{
			TransactionInfo info = new TransactionInfo();
			Transaction tx = Transaction.Current;
			if (tx != null)
			{
				info.UsingServiceSideTransaction = (tx.TransactionInformation.DistributedIdentifier == Guid.Empty);
				info.UsingClientSideTransaction = (tx.TransactionInformation.DistributedIdentifier != Guid.Empty);
				info.DistributedIdentifier = tx.TransactionInformation.DistributedIdentifier;
				info.LocalIdentifier = tx.TransactionInformation.LocalIdentifier;
				info.InstanceIdentifier = instanceId;
			}
			return info;
		}

		#region INoFlow Members

		[OperationBehavior(TransactionScopeRequired = false)]
		TransactionInfo INoFlow.ScopeNotRequired()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo INoFlow.FlowUnspecified()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo INoFlow.FlowNotAllowed()
		{
			return GetTransactionInfo();
		}

		#endregion

		#region ITxFlow Members

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo ITxFlow.FlowAllowed()
		{
			return GetTransactionInfo();
		}

		[OperationBehavior(TransactionScopeRequired = true)]
		TransactionInfo ITxFlow.FlowMandatory()
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

		public TransactionInfo ScopeNotRequired()
		{ return Channel.ScopeNotRequired(); }

		public TransactionInfo FlowUnspecified()
		{ return Channel.FlowUnspecified(); }

		public TransactionInfo FlowNotAllowed()
		{ return Channel.FlowNotAllowed(); }

		#endregion
	}

	class TxFlowClient : ClientBase<ITxFlow>, ITxFlow
	{
		public TxFlowClient(Binding binding, string remoteAddress)
			: base(binding, new EndpointAddress(remoteAddress)) { }

		#region IClientTxContract Members

		public TransactionInfo FlowAllowed()
		{ return Channel.FlowAllowed(); }

		public TransactionInfo FlowMandatory()
		{ return Channel.FlowMandatory(); }

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

		static ServiceHost<TransactionFlowService> host;
		static string noFlowAddress = "net.pipe://localhost/" + Guid.NewGuid().ToString();
		static string txFlowAddress = "net.pipe://localhost/" + Guid.NewGuid().ToString();

		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			txFlowBinding = new NetNamedPipeBinding();
			txFlowBinding.TransactionFlow = true;
			txFlowBinding.TransactionProtocol = TransactionProtocol.OleTransactions;

			noFlowBinding = new NetNamedPipeBinding();
			noFlowBinding.TransactionFlow = false;

			host = new ServiceHost<TransactionFlowService>();
			host.AddServiceEndpoint<ITxFlow>(txFlowBinding, txFlowAddress);
			host.AddServiceEndpoint<INoFlow>(txFlowBinding, noFlowAddress);
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
			NoFlowClient proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
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
			NoFlowClient proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			using (TransactionScope tx = new TransactionScope(TransactionScopeOption.RequiresNew))
			{
				TransactionInfo info = proxy.FlowUnspecified();
				Assert.IsFalse(info.UsingClientSideTransaction, "Expected client-side transaction");
				Assert.IsTrue(info.UsingServiceSideTransaction);
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
			NoFlowClient proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
			proxy.Open();
			TransactionInfo info = proxy.FlowUnspecified();
			Assert.IsFalse(info.UsingClientSideTransaction, "Expected client-side transaction");
			Assert.IsTrue(info.UsingServiceSideTransaction);
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
			NoFlowClient proxy = new NoFlowClient(noFlowBinding, noFlowAddress);
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
			TxFlowClient proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
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
			TxFlowClient proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
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
			TxFlowClient proxy = new TxFlowClient(txFlowBinding, txFlowAddress);
			proxy.Open();
			TransactionInfo info = proxy.FlowMandatory();
			proxy.Close();
		}
	}
}
