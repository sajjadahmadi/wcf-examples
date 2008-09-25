using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
    interface IEnableMetadataExchange
    {
        bool HasMetadataBehavior { get; }
        bool HttpGetEnabled { get; }
        void EnableHttpGet();
        void AddMexEndPoints();
    }

    public class ServiceHost<T> : ServiceHost, IEnableMetadataExchange
    {
        public ServiceHost()
            : base(typeof(T))
        { }
        public ServiceHost(params string[] baseAddresses)
            : base(typeof(T), Convert(baseAddresses))
        { }
        public ServiceHost(params Uri[] baseAddresses)
            : base(typeof(T), baseAddresses)
        { }
        static Uri[] Convert(string[] baseAddresses)
        {
            Converter<string, Uri> convert = delegate(string address)
            {
                return new Uri(address);
            };
            return Array.ConvertAll(baseAddresses, convert);
        }

        #region IEnableMetadataExchange Members
        public bool HttpGetEnabled
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

        public void EnableHttpGet()
        {
            if (State == CommunicationState.Opened)
            {
                throw new InvalidOperationException("Host is already open");
            }
            ServiceMetadataBehavior metadataBehavior;
            metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                metadataBehavior.HttpGetEnabled = true;
                Description.Behaviors.Add(metadataBehavior);
            }
        }

        public bool HasMetadataBehavior
        {
            get
            {
                return Description.Behaviors.Any(b => b == typeof(ServiceMetadataBehavior));
            }
        }

        public void AddMexEndPoints()
        {
            if (!HasMetadataBehavior)
            {
                throw new InvalidOperationException("Must have ServiceMetadataBehavior");
            }
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
        #endregion
    }
}
