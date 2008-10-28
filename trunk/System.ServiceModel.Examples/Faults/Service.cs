using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{

    [ServiceContract]
    interface IMyContract
    {
        [OperationContract]
        [FaultContract(typeof(FaultType))]
        void ThrowTypedFault();

        [OperationContract]
        void ThrowUntypedFault();

        [OperationContract]
        void ThrowClrException();
    }

    [ServiceContract(CallbackContract = typeof(IMyContract2Callback))]
    interface IMyContract2
    {
        [OperationContract]
        bool CallbackAndCatchFault();
    }

    interface IMyContract2Callback
    {
        [OperationContract]
        [FaultContract(typeof(InvalidOperationException))]
        void OnCallback();
    }

    [DataContract]
    class FaultType
    {
        private string description;

        public FaultType(string description)
        { this.description = description; }

        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class MyService : IMyContract, IMyContract2
    {
        public void ThrowTypedFault()
        {
            try
            {
                throw new ApplicationException("Some application error.");
            }
            catch (ApplicationException ex)
            {
                FaultType fault = new FaultType(ex.Message);
                throw new FaultException<FaultType>(fault);
            }
        }

        public void ThrowUntypedFault()
        {
            throw new FaultException("Untyped Fault.");
        }

        public void ThrowClrException()
        {
            throw new NotImplementedException();
        }

        public bool CallbackAndCatchFault()
        {
            IMyContract2Callback callback =
                OperationContext.Current.GetCallbackChannel<IMyContract2Callback>();

            try
            { callback.OnCallback(); }
            catch (FaultException)
            { return true; }

            return false;
        }
    }
}
