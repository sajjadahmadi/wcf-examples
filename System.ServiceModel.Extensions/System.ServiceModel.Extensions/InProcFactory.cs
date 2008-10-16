using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace System.ServiceModel
{
    public static class InProcFactory
    {
        struct HostRecord
        {
            public readonly ServiceHost Host;
            public readonly string Address;
            public HostRecord(ServiceHost host, string address)
            {
                Host = host;
                Address = address;
            }
        }
        static readonly Uri BaseAddress = new Uri("net.pipe://localhost/");
        static readonly Binding NamedPipeBinding;
        static Dictionary<Type, HostRecord> m_Hosts = new Dictionary<Type, HostRecord>();

        static InProcFactory()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.TransactionFlow = true;
            NamedPipeBinding = binding;
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                foreach (KeyValuePair<Type, HostRecord> pair in m_Hosts)
                {
                    pair.Value.Host.Close();
                }
            };
        }

        static HostRecord GetHostRecord<TService, TContract>()
            where TService : TContract
            where TContract : class
        {
            HostRecord hostRecord;
            if (m_Hosts.ContainsKey(typeof(TService)))
            {
                hostRecord = m_Hosts[typeof(TService)];
            }
            else
            {
                ServiceHost host = new ServiceHost(typeof(TService), BaseAddress);
                string address = BaseAddress.ToString() + Guid.NewGuid().ToString();
                hostRecord = new HostRecord(host, address);
                m_Hosts.Add(typeof(TService), hostRecord);
                host.AddServiceEndpoint(typeof(TContract), NamedPipeBinding, address);
                host.Open();
            }
            return hostRecord;
        }

        public static TContract CreateInstance<TService, TContract>()
            where TService : TContract
            where TContract : class
        {
            HostRecord hostRecord = GetHostRecord<TService, TContract>();
            return ChannelFactory<TContract>.CreateChannel(NamedPipeBinding, new EndpointAddress(hostRecord.Address));
        }

        public static void CloseProxy<I>(I instance) where I : class
        {
            ICommunicationObject proxy = instance as ICommunicationObject;
            Debug.Assert(proxy != null);
            proxy.Close();
        }
    }
}