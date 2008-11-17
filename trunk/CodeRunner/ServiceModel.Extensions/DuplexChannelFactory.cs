using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel
{
    public class DuplexChannelFactory<T, C> : DuplexChannelFactory<T>
          where T : class
    {
        static DuplexChannelFactory()
        { DuplexClientBase<T, C>.ValidateCallback(); }
        public DuplexChannelFactory() : base(typeof(C)) { }
        public DuplexChannelFactory(ServiceEndpoint endpoint) : base(typeof(C), endpoint) { }
        public DuplexChannelFactory(Binding binding) : base(typeof(C), binding) { }
        public DuplexChannelFactory(Binding binding, EndpointAddress remoteAddress) : base(typeof(C), binding, remoteAddress) { }
        public DuplexChannelFactory(Binding binding, string remoteAddress) : base(typeof(C), binding, remoteAddress) { }
        public DuplexChannelFactory(string endpointConfigurationName) : base(typeof(C), endpointConfigurationName) { }
        public DuplexChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress) : base(typeof(C), endpointConfigurationName, remoteAddress) { }
        public DuplexChannelFactory(InstanceContext<C> callback) : base(callback.Context) { }
        public DuplexChannelFactory(InstanceContext<C> callback, ServiceEndpoint endpoint) : base(callback.Context, endpoint) { }
        public DuplexChannelFactory(InstanceContext<C> callback, Binding binding) : base(callback.Context, binding) { }
        public DuplexChannelFactory(InstanceContext<C> callback, Binding binding, EndpointAddress remoteAddress) : base(callback.Context, binding, remoteAddress) { }
        public DuplexChannelFactory(InstanceContext<C> callback, Binding binding, string remoteAddress) : base(callback.Context, binding, remoteAddress) { }
        public DuplexChannelFactory(InstanceContext<C> callback, string endpointConfigurationName) : base(callback.Context, endpointConfigurationName) { }
        public DuplexChannelFactory(InstanceContext<C> callback, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callback.Context, endpointConfigurationName, remoteAddress) { }
        public DuplexChannelFactory(C callback) : base(callback) { }
        public DuplexChannelFactory(C callback, ServiceEndpoint endpoint) : base(callback, endpoint) { }
        public DuplexChannelFactory(C callback, Binding binding) : base(callback, binding) { }
        public DuplexChannelFactory(C callback, Binding binding, EndpointAddress remoteAddress) : base(callback, binding, remoteAddress) { }
        public DuplexChannelFactory(C callback, Binding binding, string remoteAddress) : base(callback, binding, remoteAddress) { }
        public DuplexChannelFactory(C callback, string endpointConfigurationName) : base(callback, endpointConfigurationName) { }
        public DuplexChannelFactory(C callback, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callback, endpointConfigurationName, remoteAddress) { }

        public static T CreateChannel(C callback, string endpointName)
        { return DuplexChannelFactory<T>.CreateChannel(callback, endpointName); }
        public static T CreateChannel(C callback, Binding binding, EndpointAddress endpointAddress)
        { return DuplexChannelFactory<T>.CreateChannel(callback, binding, endpointAddress); }
        public static T CreateChannel(InstanceContext<C> callback, string endpointName)
        { return DuplexChannelFactory<T>.CreateChannel(callback, endpointName); }
        public static T CreateChannel(InstanceContext<C> callback, Binding binding, EndpointAddress endpointAddress)
        { return DuplexChannelFactory<T>.CreateChannel(callback, binding, endpointAddress); }
    }
}
