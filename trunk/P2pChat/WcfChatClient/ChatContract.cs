using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;

namespace Microsoft.ServiceModel.Samples
{
    [ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples", CallbackContract = typeof(IChat))]
    public interface IChat
    {
        [OperationContract(IsOneWay = true)]
        void Join(string member);
        [OperationContract(IsOneWay = true)]
        void Chat(string member, string message);
        [OperationContract(IsOneWay = true)]
        void Leave(string member);
    }

    public interface IChatChannel : IChat, IClientChannel
    {}
}
