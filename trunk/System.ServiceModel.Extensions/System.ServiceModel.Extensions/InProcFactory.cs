using System.Collections.Generic;
namespace System.ServiceModel
{
    // TODO: test
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

        static HostRecord GetHostRecord<S, I>()
            where I : class
            where S : I
        {
            HostRecord hostRecord;
            if (m_Hosts.ContainsKey(typeof(S)))
            {
                hostRecord = m_Hosts[typeof(S)];
            }
            else
            {
                ServiceHost host = new ServiceHost(typeof(S), BaseAddress);
                string address = BaseAddress.ToString() + Guid.NewGuid().ToString();
                hostRecord = new HostRecord(host, address);
                m_Hosts.Add(typeof(S), hostRecord);
                host.AddServiceEndpoint(typeof(I), NamedPipeBinding, address);
                host.Open();
            }
            return hostRecord;
        }

        public static I CreateInstance<S, I>()
            where I : class
            where S : I
        {
            HostRecord hostRecord = GetHostRecord<S, I>();
            return ChannelFactory<I>.CreateChannel(NamedPipeBinding, new EndpointAddress(hostRecord.Address));
        }
    }
}