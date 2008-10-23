using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;

namespace System.ServiceModel
{
    interface IEnableMetadataExchange
    {
        bool MetadataExchangeEnabled { get; }
        void EnableMetadataExchange();
    }
    interface IThrottling
    {
        void SetThrottle(System.ServiceModel.Description.ServiceThrottlingBehavior throttleBehavior);
        void SetThrottle(int maxCalls, int maxSessions, int maxInstances);
        void SetThrottle(System.ServiceModel.Description.ServiceThrottlingBehavior throttleBehavior, bool overrideConfig);
        System.ServiceModel.Description.ServiceThrottlingBehavior ThrottleBehavior { get; }
    }
    interface IInProcFactory
    {
        TContract CreateChannel<TContract>(Binding binding, string address)
            where TContract : class;
    }

    public class ServiceHost<TService> :
        ServiceHost,
        IEnableMetadataExchange,
        IThrottling,
        IInProcFactory
    {
        private const string HOSTOPEN = "Host is already open";

        public ServiceHost()
            : base(typeof(TService))
        { }
        public ServiceHost(params string[] baseAddresses)
            : base(typeof(TService), ConvertToUri(baseAddresses))
        { }
        public ServiceHost(params Uri[] baseAddresses)
            : base(typeof(TService), baseAddresses)
        { }
        public ServiceHost(TService singleton, params string[] baseAddresses)
            : base(singleton, ConvertToUri(baseAddresses))
        { }
        public ServiceHost(TService singleton, params Uri[] baseAddresses)
            : base(singleton, baseAddresses)
        { }

        static Uri[] ConvertToUri(string[] baseAddresses)
        {
            Converter<string, Uri> convert = delegate(string address)
                { return new Uri(address); };
            return Array.ConvertAll(baseAddresses, convert);
        }

        public ServiceEndpoint AddServiceEndpoint<TContract>(Binding binding, string address)
        { return base.AddServiceEndpoint(typeof(TContract), binding, address); }

        #region IEnableMetadataExchange
        public bool MetadataExchangeEnabled
        {
            get
            {
                ServiceMetadataBehavior metadataBehavior;
                metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null)
                {
                    return false;
                }
                return metadataBehavior.HttpGetEnabled;
            }
        }

        public void EnableMetadataExchange()
        {
            if (State == CommunicationState.Opened)
            {
                throw new InvalidOperationException(HOSTOPEN);
            }
            ServiceMetadataBehavior metadataBehavior;
            metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                metadataBehavior.HttpGetEnabled = true;
                Description.Behaviors.Add(metadataBehavior);
            }
            AddMexEndPoints();
        }

        void AddMexEndPoints()
        {
            System.Diagnostics.Debug.Assert(HasMexEndpoint == false);
            foreach (Uri baseAddress in BaseAddresses)
            {
                BindingElement bindingElement = null;
                switch (baseAddress.Scheme)
                {
                    case "net.tcp":
                        {
                            bindingElement = new TcpTransportBindingElement();
                            break;
                        }
                    case "net.pipe":
                        {
                            bindingElement = new NamedPipeTransportBindingElement();
                            break;
                        }
                    case "http":
                        {
                            bindingElement = new HttpTransportBindingElement();
                            break;
                        }
                    case "https":
                        {
                            bindingElement = new HttpsTransportBindingElement();
                            break;
                        }
                }
                if (bindingElement != null)
                {
                    Binding binding = new CustomBinding(bindingElement);
                    AddServiceEndpoint(typeof(IMetadataExchange), binding, "MEX");
                }
            }
        }

        bool HasMexEndpoint
        {
            get
            {
                return Description.Endpoints.Any(ep =>
                    ep.Contract.ContractType == typeof(IMetadataExchange));
            }
        }
        #endregion IEnableMetadataExchange

        public virtual TService Singleton
        {
            get
            {
                if (SingletonInstance == null) { return default(TService); }
                return (TService)SingletonInstance;
            }
        }

        #region IThrottling

        public void SetThrottle(int maxCalls, int maxSessions, int maxInstances)
        {
            ServiceThrottlingBehavior throttleBehavior = new ServiceThrottlingBehavior();
            throttleBehavior.MaxConcurrentCalls = maxCalls;
            throttleBehavior.MaxConcurrentSessions = maxSessions;
            throttleBehavior.MaxConcurrentInstances = maxInstances;
            SetThrottle(throttleBehavior);
        }

        public void SetThrottle(ServiceThrottlingBehavior throttleBehavior)
        { SetThrottle(throttleBehavior, false); }

        public void SetThrottle(ServiceThrottlingBehavior throttleBehavior, bool overrideConfig)
        {
            if (State == CommunicationState.Opened)
            { throw new InvalidOperationException(HOSTOPEN); }

            ServiceThrottlingBehavior exitingThrottle = this.ThrottleBehavior;

            if (exitingThrottle != null && overrideConfig == false)
            { return; }

            if (exitingThrottle != null && overrideConfig == true)
            { Description.Behaviors.Remove(exitingThrottle); }

            Description.Behaviors.Add(throttleBehavior);
        }

        public ServiceThrottlingBehavior ThrottleBehavior
        {
            get { return Description.Behaviors.Find<ServiceThrottlingBehavior>(); }
        }

        #endregion IThrottling

        #region IInProcFactory Members

        public TContract CreateChannel<TContract>(Binding binding, string address)
            where TContract : class
        {
            return ChannelFactory<TContract>.CreateChannel(binding, new EndpointAddress(address));
        }

        #endregion
    }
}
