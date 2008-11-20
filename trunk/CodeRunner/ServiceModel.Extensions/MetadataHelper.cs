using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace System.ServiceModel.Extensions
{
    public static class MetadataHelper
    {
        const int MessageSizeMultiplier = 5;

        static ServiceEndpointCollection GetEndpoints(
            string mexAddress,
            BindingElement bindingElement)
        {
            CustomBinding binding = new CustomBinding(bindingElement);

            MetadataExchangeClient MEXClient = new MetadataExchangeClient(binding);
            MetadataSet metadata = MEXClient.GetMetadata(new EndpointAddress(mexAddress));

            MetadataImporter importer = new WsdlImporter(metadata);
            return importer.ImportAllEndpoints();
        }

        public static ServiceEndpointCollection GetEndpoints(string mexAddress)
        {
            Uri address = new Uri(mexAddress);
            ServiceEndpointCollection endpoints = null;

            switch (address.Scheme)
            {
                case "net.tcp":
                    TcpTransportBindingElement tcpBindingElement =
                        new TcpTransportBindingElement();
                    tcpBindingElement.MaxReceivedMessageSize *= MessageSizeMultiplier;
                    endpoints = GetEndpoints(mexAddress, tcpBindingElement);
                    return endpoints;
                case "net.pipe":
                    NamedPipeTransportBindingElement pipeBindingElement =
                        new NamedPipeTransportBindingElement();
                    pipeBindingElement.MaxReceivedMessageSize *= MessageSizeMultiplier;
                    endpoints = GetEndpoints(mexAddress, pipeBindingElement);
                    return endpoints;
                default:
                    throw new NotImplementedException("Support for scheme type not implemented.");
            }
        }

        public static bool SupportsContract(
            string mexAddress,
            Type contractType)
        {
            if (contractType.IsInterface == false)
            {
                throw new ArgumentException(contractType.Name + " is not an interface", "contractType");
            }
            object[] attributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute), false);
            if (attributes.Length == 0)
            {
                throw new ArgumentException("Interface does not have the ServiceContractAttribute", "contractType");
            }
            ServiceContractAttribute attribute = attributes[0] as ServiceContractAttribute;
            if (attribute.Name == null)
            {
                attribute.Name = contractType.Name;
            }
            if (attribute.Namespace == null)
            {
                attribute.Namespace = "http://tempuri.org/";
            }
            return SupportsContract(mexAddress, attribute.Namespace, attribute.Name);
        }

        public static bool SupportsContract(
            string mexAddress,
            string contractNamespace,
            string contractName)
        {
            if (string.IsNullOrEmpty(contractNamespace))
            {
                throw new ArgumentException("Empty namespace", "contractNamespace");
            }
            if (string.IsNullOrEmpty(contractName))
            {
                throw new ArgumentException("Empty name", "contractName");
            }
            ServiceEndpointCollection endpoints = GetEndpoints(mexAddress);
            foreach (ServiceEndpoint endpoint in endpoints)
            {
                if (endpoint.Contract.Namespace == contractNamespace &&
                    endpoint.Contract.Name == contractName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
