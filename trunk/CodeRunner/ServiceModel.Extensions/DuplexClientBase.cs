using System.ServiceModel.Channels;

namespace System.ServiceModel
{
    // Type Safe DuplexClientBase
    public abstract class DuplexClientBase<TChannel, TCallback> : DuplexClientBase<TChannel> where TChannel : class
    {
        protected DuplexClientBase(InstanceContext<TCallback> callback) : base(callback.Context) { }
        protected DuplexClientBase(InstanceContext<TCallback> callback, Binding binding, EndpointAddress remoteAddress) : base(callback.Context, binding, remoteAddress) { }
        protected DuplexClientBase(InstanceContext<TCallback> callback, string endpointConfigurationName) : base(callback.Context, endpointConfigurationName) { }
        protected DuplexClientBase(InstanceContext<TCallback> callback, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callback.Context, endpointConfigurationName, remoteAddress) { }
        protected DuplexClientBase(InstanceContext<TCallback> callback, string endpointConfigurationName, string remoteAddress) : base(callback.Context, endpointConfigurationName, remoteAddress) { }
        protected DuplexClientBase(TCallback callback) : base(callback) { }
        protected DuplexClientBase(TCallback callback, Binding binding, EndpointAddress remoteAddress) : base(callback, binding, remoteAddress) { }
        protected DuplexClientBase(TCallback callback, string endpointConfigurationName) : base(callback, endpointConfigurationName) { }
        protected DuplexClientBase(TCallback callback, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callback, endpointConfigurationName, remoteAddress) { }
        protected DuplexClientBase(TCallback callback, string endpointConfigurationName, string remoteAddress) : base(callback, endpointConfigurationName, remoteAddress) { }
        static DuplexClientBase() { ValidateCallback(); }
        internal static void ValidateCallback()
        {
            Type contractType = typeof(TChannel);
            Type callbackType = typeof(TCallback);

            object[] attribs = contractType.GetCustomAttributes(typeof(ServiceContractAttribute), false);
            if (attribs.Length != 1)
            { throw new InvalidOperationException("Type of " + contractType + " is not a service contract."); }

            ServiceContractAttribute contractAttrib = attribs[0] as ServiceContractAttribute;
            if (contractAttrib.CallbackContract != callbackType)
            { throw new InvalidOperationException("Type of " + callbackType + " is not configured as callback contract for " + contractType); }
        }
    }
}
