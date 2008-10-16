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
        bool MetadataExchangeEnabled { get; }
        void EnableMetadataExchange();
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
        #endregion
    }


    //class LocalHost<TService> : ServiceHost<TService>
    //{
    //    public LocalHost()
    //        : base()
    //    { }
    //    public LocalHost(params string[] baseAddresses)
    //        : base(baseAddresses)
    //    { }
    //    public LocalHost(params Uri[] baseAddresses)
    //        : base(baseAddresses)
    //    { }

    //    public ServiceEndpoint GetInProcEndpoint<TBinding>()
    //        where TBinding : Binding, new()
    //    {
    //        ServiceEndpoint ep = this.Description.Endpoints.FirstOrDefault(e =>
    //            e.Binding.GetType() == typeof(TBinding));
    //        if (ep == null)
    //        {
    //            AddInProcEndpoint<TBinding>();
    //        }
    //        return ep;
    //    }

    //    public ServiceEndpoint AddInProcEndpoint<TBinding>()
    //        where TBinding : Binding, new()
    //    {
    //        string baseAddress;
    //        Binding binding;
    //        string endpointAddress;

    //        if (typeof(TBinding) == typeof(NetNamedPipeBinding))
    //        {
    //            baseAddress = "net.pipe://localhost/";
    //            binding = new NetNamedPipeBinding();
    //            ((NetNamedPipeBinding)binding).TransactionFlow = true;
    //        }
    //        else if (typeof(TBinding) == typeof(BasicHttpBinding))
    //        {
    //            baseAddress = "http://localhost/";
    //            binding = new BasicHttpBinding();
    //            //((BasicHttpBinding)binding).TransactionFlow = true;
    //        }
    //        else throw new ArgumentOutOfRangeException("TBinding", "Binding type not supported... Yet.");

    //        Uri uri = new Uri(baseAddress);
    //        if (Description == null)
    //        {
    //            UriSchemeKeyedCollection c = new UriSchemeKeyedCollection(uri);
    //            InitializeDescription(typeof(TService), c);
    //        }
    //        else
    //            AddBaseAddress(uri);
    //        endpointAddress = baseAddress + Guid.NewGuid().ToString();
    //        AddServiceEndpoint( typeof(TContract), binding, endpointAddress);
    //        return null;
    //    }
    //}
}
