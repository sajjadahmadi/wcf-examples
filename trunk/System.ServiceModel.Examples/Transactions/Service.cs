using System.ServiceModel.Channels;
using System.Transactions;
using System.Diagnostics;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    interface IClientTxNotAllowed
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.NotAllowed)]
        void MyMethod();
    }
    [ServiceContract]
    interface IClientTxAllowed
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void MyMethod();
    }
    [ServiceContract]
    interface IClientTxMandatory
    {
        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Mandatory)]
        void MyMethod();
    }


    [BindingRequirementAttribute(TransactionFlowEnabled=true)]
    class TxService : IClientTxNotAllowed, IClientTxAllowed, IClientTxMandatory
    {
        [OperationBehavior(TransactionScopeRequired = true)]
        void IClientTxNotAllowed.MyMethod()
        {
            Debug.Assert(Transaction.Current != null);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        void IClientTxAllowed.MyMethod()
        {
            Debug.Assert(Transaction.Current != null);
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        void IClientTxMandatory.MyMethod()
        {
            Debug.Assert(Transaction.Current != null);
        }
    }

}
