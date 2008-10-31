using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;
using System.ServiceModel.Errors;

namespace System.ServiceModel
{
    #region Interfaces
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
    interface IFaultBehavior
    {
        bool IncludeExceptionDetailInFaults { get; set; }
    }
    #endregion

    public class ServiceHost<TService> :
        ServiceHost,
        IEnableMetadataExchange,
        IThrottling,
        IInProcFactory,
        IFaultBehavior
    {
        private const string hostAlreadyOpen = "Host is already open";

        #region Constructors
        public ServiceHost()
            : this(new Uri[] { })
        { }
        public ServiceHost(params string[] baseAddresses)
            : this(ConvertToUri(baseAddresses))
        { }
        public ServiceHost(params Uri[] baseAddresses)
            : base(typeof(TService), baseAddresses)
        { }
        public ServiceHost(TService singleton)
            : this(singleton, new Uri[] { })
        { }
        public ServiceHost(TService singleton, params string[] baseAddresses)
            : this(singleton, ConvertToUri(baseAddresses))
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
        #endregion

        #region Error Handling
        class ErrorHandlerBehavior : IServiceBehavior, IErrorHandler
        {
            IErrorHandler errorHandler;

            public ErrorHandlerBehavior(IErrorHandler errorHandler)
            { this.errorHandler = errorHandler; }

            //IServiceBehavior Members
            void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase host, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
            { throw new NotImplementedException(); }
            void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase host)
            {
                foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
                { dispatcher.ErrorHandlers.Add(this); }
            }
            void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase host)
            { throw new NotImplementedException(); }

            // IErrorHandler Members
            bool IErrorHandler.HandleError(Exception error)
            { return errorHandler.HandleError(error); }
            void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
            { errorHandler.ProvideFault(error, version, ref fault); }
        }

        List<IServiceBehavior> errorHandlers = new List<IServiceBehavior>();
        public void AddErrorHandler(IErrorHandler errorHandler)
        {
            if (State == CommunicationState.Opened)
            { throw new InvalidOperationException(hostAlreadyOpen); }
            IServiceBehavior errorHandlerBehavior = new ErrorHandlerBehavior(errorHandler);
            errorHandlers.Add(errorHandlerBehavior);
        }
        //TODO: Implement AddErrorHandler().
        //public void AddErrorHandler()
        //{
        //    if (State == CommunicationState.Opened)
        //    { throw new InvalidOperationException(hostAlreadyOpen); }
        //    IServiceBehavior errorHandlerBehavior = new ErrorHandlerBehaviorAttribute();
        //    errorHandlers.Add(errorHandlerBehavior);
        //}
        #endregion

        public ServiceEndpoint AddServiceEndpoint<TContract>(Binding binding, string address)
        { return base.AddServiceEndpoint(typeof(TContract), binding, address); }

        public virtual TService Singleton
        {
            get { return (TService)SingletonInstance; }
        }

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
                throw new InvalidOperationException(hostAlreadyOpen);
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
            { throw new InvalidOperationException(hostAlreadyOpen); }

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

        public TContract CreateChannel<TContract, TCallback>(TCallback callback, Binding binding, string address)
            where TContract : class
        {
            return DuplexChannelFactory<TContract, TCallback>
                    .CreateChannel(callback, binding, new EndpointAddress(address));
        }

        #endregion

        #region IFaultBehavior Members

        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                ServiceBehaviorAttribute behavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
                return behavior.IncludeExceptionDetailInFaults;
            }
            set
            {
                if (State == CommunicationState.Opened)
                { throw new InvalidOperationException(hostAlreadyOpen); }
                ServiceBehaviorAttribute behavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
                behavior.IncludeExceptionDetailInFaults = value;
            }
        }

        #endregion
    }
}
