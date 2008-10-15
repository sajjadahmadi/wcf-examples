using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Linq;

namespace System.ServiceModel
{
    public static class ChannelFactory<TService, TContract>
        where TService : TContract
        where TContract : class
    {
        static List<ServiceHost> _hosts = new List<ServiceHost>();

        class HostInfo
        {
            public readonly ServiceHost Host;
            public readonly string Address;
            public HostInfo(ServiceHost host, string address)
            {
                Host = host;
                Address = address;
            }
        }

        static ChannelFactory()
        {
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                foreach (ServiceHost host in _hosts)
                {
                    host.Close();
                }
            };
        }

        static ServiceHost GetServiceHost(Binding binding, Uri baseAddress)
        {
            ServiceHost host;
            host = _hosts.Find(r =>
                r.Description.ServiceType == typeof(TService));
            {
                host = new ServiceHost(typeof(TService), baseAddress);
                _hosts.Add(host);
            }

            Description.ServiceEndpoint ep;
            ep = host.Description.Endpoints.FirstOrDefault(e =>
                e.Binding.GetType() == binding.GetType());
            if (ep == null)
            {
                string address = baseAddress.ToString() + Guid.NewGuid().ToString();
                host.AddServiceEndpoint(typeof(TContract), binding, address);
                host.Open();
            }

            return host;
        }

        public static TContract CreateChannel()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.TransactionFlow = true;
            Uri baseAddress = new Uri("net.pipe://localhost/");

            ServiceHost host = GetServiceHost(binding, baseAddress);
            return ChannelFactory<TContract>.CreateChannel(binding, host.Description.Endpoints.First().Address);
        }

        public static void CloseChannel(TContract instance)
        {
            ICommunicationObject proxy = instance as ICommunicationObject;
            Debug.Assert(proxy != null);
            proxy.Close();
        }
    }
}