using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Faults
{
    [ServiceContract]
    public interface IMyContract
    {
        [OperationContract]
        [FaultContract(typeof(FaultType))]
        void ThrowTypedFault();

        [OperationContract]
        void ThrowUntypedFault();
    }

    [DataContract]
    public class FaultType
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

    public class MyService : IMyContract
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
    }

}
