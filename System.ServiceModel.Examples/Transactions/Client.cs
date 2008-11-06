using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{

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

}