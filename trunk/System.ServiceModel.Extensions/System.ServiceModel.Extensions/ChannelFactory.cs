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
        static Dictionary<Type, ServiceHost> _hosts = new Dictionary<Type, ServiceHost>();

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

        static ServiceHost GetLocalHost<TBinding>()
            where TBinding : Binding
        {
            ServiceHost host;
            if (_hosts.ContainsKey(typeof(TService)))
            {
                host = _hosts[typeof(TService)];
            }
            else
            {
                host = CreateLocalHost<TBinding>();
                _hosts.Add(typeof(TService), host);
            }

            return host;
        }

        private static ServiceHost CreateLocalHost<TBinding>()
            where TBinding : Binding
        {
            Uri baseAddress;
            string address;

            if (typeof(TBinding) == typeof(NetNamedPipeBinding))
            {
                baseAddress = new Uri("net.pipe://localhost/");
                NetNamedPipeBinding pipeBinding = (NetNamedPipeBinding)binding;
                pipeBinding.TransactionFlow = true;
                address = baseAddress.ToString() + Guid.NewGuid().ToString();
            }
            else throw new ArgumentOutOfRangeException("binding", "Binding type not implemented.");

            ServiceHost host;
            host = new ServiceHost(typeof(TService), baseAddress);
            host.AddServiceEndpoint(typeof(TContract), binding, address);
            host.Open();
            return host;
        }

        public static TContract CreateChannel()
        {
            return CreateChannel<NetNamedPipeBinding>();
        }

        public static TContract CreateChannel<TBinding>()
            where TBinding : Binding, new()
        {
            ServiceHost host = GetLocalHost<TBinding>();
            ServiceEndpoint ep = host.Description.Endpoints.First();
            return CreateChannel(ep.Binding, ep.Address);
        }
        public static TContract CreateChannel(Binding binding, EndpointAddress endpointAddress)
        {
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