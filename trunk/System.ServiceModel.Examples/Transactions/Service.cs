using System.ServiceModel.Channels;
using System.Transactions;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{
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


    [BindingRequirementAttribute(TransactionFlowRequired = true)]
    partial class MyService : INoFlow, ITxFlow
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


}
