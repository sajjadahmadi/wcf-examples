using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace System.ServiceModel
{
    public class ServiceHost<T> : ServiceHost
    {
        public ServiceHost() : base(typeof(T)) 
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
    }
}
