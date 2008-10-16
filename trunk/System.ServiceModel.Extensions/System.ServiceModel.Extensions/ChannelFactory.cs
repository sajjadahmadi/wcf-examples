using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Description;

namespace System.ServiceModel
{
    public static class ChannelFactory<TService, TContract>
        where TService : TContract
        where TContract : class
    {
        static Dictionary<Type, ServiceHost<TService>> _hosts = new Dictionary<Type, ServiceHost<TService>>();

        static ChannelFactory()
        {
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                foreach (ServiceHost host in _hosts.Values)
                {
                    host.Close();
                }
            };
        }

        static ServiceHost<TService> GetServiceHost()
        {
            ServiceHost<TService> host;
            if (_hosts.ContainsKey(typeof(TService)))
            {
                host = _hosts[typeof(TService)];
            }
            else
            {
                host = CreateServiceHost();
                _hosts.Add(typeof(TService), host);
            }
            return host;
        }
        static ServiceHost<TService> CreateServiceHost()
        {
            ServiceHost<TService> host = new ServiceHost<TService>();
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.TransactionFlow = true;
            string endpointAddress = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            host.AddServiceEndpoint(typeof(TContract), binding, endpointAddress);
            host.Open();
            return host;
        }

        public static TContract CreateChannel()
        {
            ServiceHost<TService> host = GetServiceHost();
            ServiceEndpoint ep = host.Description.Endpoints.FirstOrDefault(e =>
                e.Binding.GetType() == typeof(NetNamedPipeBinding));
            return CreateChannel(ep.Binding, ep.Address);
        }
        public static TContract CreateChannel(Binding binding, string uri)
        { return CreateChannel(binding, new EndpointAddress(uri)); }
        public static TContract CreateChannel(Binding binding, EndpointAddress endpointAddress)
        {
            ServiceHost<TService> host = GetServiceHost();
            ServiceEndpoint ep = host.Description.Endpoints.FirstOrDefault(e =>
                e.Binding.GetType() == binding.GetType() &&
                e.Address.Uri == endpointAddress.Uri
            );
            if (ep == null)
                host.AddServiceEndpoint(typeof(TContract), binding, endpointAddress.Uri);
            return ChannelFactory<TContract>.CreateChannel(binding, endpointAddress);
        }

        public static void CloseChannel(TContract instance)
        {
            ICommunicationObject proxy = instance as ICommunicationObject;
            Debug.Assert(proxy != null);
            proxy.Close();
        }
    }
}