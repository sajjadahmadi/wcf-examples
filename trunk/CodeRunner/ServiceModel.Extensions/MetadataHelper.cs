using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Extensions
{
    public static class MetadataHelper
    {
        const int MessageSizeMultiplier = 5;

        static ServiceEndpointCollection GetEndpoints(
            Uri mexUri,
            BindingElement bindingElement)
        {
            CustomBinding binding = new CustomBinding(bindingElement);

            MetadataExchangeClient MEXClient = new MetadataExchangeClient(binding);
            MetadataSet metadata = MEXClient.GetMetadata(new EndpointAddress(mexUri));

            MetadataImporter importer = new WsdlImporter(metadata);
            return importer.ImportAllEndpoints();
        }

        public static ServiceEndpointCollection GetEndpoints(string mexAddress)
        {
            Uri mexUri = new Uri(mexAddress);
            TransportBindingElement transportElement;
            switch (mexUri.Scheme)
            {
                case "net.tcp":
                    transportElement = new TcpTransportBindingElement();
                    break;
                case "net.pipe":
                    transportElement = new NamedPipeTransportBindingElement();
                    break;
                case "http":
                    transportElement = new HttpTransportBindingElement();
                    break;
                case "https":
                    transportElement = new HttpsTransportBindingElement();
                    break;
                default:
                    throw new NotSupportedException("Scheme not supported.");
            }
            transportElement.MaxReceivedMessageSize *= MessageSizeMultiplier;
            return GetEndpoints(mexUri, transportElement); ;
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
