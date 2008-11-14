﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    class MyContractClient : ClientBase<IMyContract>, IMyContract
    {
        public MyContractClient() { }
        public MyContractClient(Binding binding, string remoteAddress) :
            base(binding, new EndpointAddress(remoteAddress)) { }

        public void ThrowTypedFault()
        { Channel.ThrowTypedFault(); }

        public void ThrowUntypedFault()
        { Channel.ThrowUntypedFault(); }

        public void ThrowClrException()
        { Channel.ThrowClrException(); }
    }

    class MyClientWithCallback : DuplexClientBase<IContractWithCallback, ICallbackContract>, IContractWithCallback
    {
        public MyClientWithCallback(
            ICallbackContract callback,
            Binding binding,
            string remoteAddress) :
            base(callback, binding, new EndpointAddress(remoteAddress)) { }

        public bool CallbackAndCatchFault()
        { return Channel.CallbackAndCatchFault(); }
    }

    class MyContractCallback : ICallbackContract
    {
        public void OnCallback()
        {
            ApplicationException ex = new ApplicationException("Callback threw this exception.");
            throw new FaultException(ex.Message);
        }
    }

}