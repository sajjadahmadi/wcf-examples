using System.ServiceModel.Channels;
using System.Transactions;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{
    [DataContract]
    struct TransactionInfo
    {        
        [DataMember]
        public bool HasTransaction { get; set; }

        [DataMember]
        public Guid DistributedIdentifier { get; set; }

        [DataMember]
        public string LocalIdentifier { get; set; }
    }

    [ServiceContract]
    interface IServiceTxContract
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.NotAllowed)]
        TransactionInfo ClientTxNotAllowedMethod();

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        TransactionInfo ClientTxAllowedMethod();
    }


    [ServiceContract]
    interface IClientTxContract
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        TransactionInfo ClientTxAllowedMethod();

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        TransactionInfo ClientTxMandatoryMethod();
    }


    [BindingRequirementAttribute(TransactionFlowRequired = true)]
    class TxService : IServiceTxContract, IClientTxContract
    {
        TransactionInfo GetTransactionInfo()
        {
            TransactionInfo info = new TransactionInfo();
            Transaction tx = Transaction.Current;
            if (tx != null)
            {
                info.HasTransaction = true;
                info.DistributedIdentifier = tx.TransactionInformation.DistributedIdentifier;
                info.LocalIdentifier = tx.TransactionInformation.LocalIdentifier;
            }
            return info;
        }

        #region IServiceTxContract Members

        [OperationBehavior(TransactionScopeRequired = true)]
        TransactionInfo IServiceTxContract.ClientTxNotAllowedMethod()
        {
            return GetTransactionInfo();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        TransactionInfo IServiceTxContract.ClientTxAllowedMethod()
        {
            return GetTransactionInfo();
        }

        #endregion

        #region IClientTxContract Members

        [OperationBehavior(TransactionScopeRequired = true)]
        TransactionInfo IClientTxContract.ClientTxAllowedMethod()
        {
            return GetTransactionInfo();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        TransactionInfo IClientTxContract.ClientTxMandatoryMethod()
        {
            return GetTransactionInfo();
        }

        #endregion

    }

}
