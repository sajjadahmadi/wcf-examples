using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
   public class ServiceHost<T> : ServiceHost, IEnableMetaDataExchange
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

      #region IEnableMetaDataExchange Members
      public bool EnableMetaDataExchange
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
         set
         {
            if (State == CommunicationState.Opened)
            {
               throw new InvalidOperationException("Host is already open");
            }
            ServiceMetadataBehavior metadataBehavior;
            metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();
            // If no MEX endpoint is available, add a MEX endpoint for each registered base address scheme
            if (metadataBehavior == null)
            {
               metadataBehavior = new ServiceMetadataBehavior();
               metadataBehavior.HttpGetEnabled = value;
               Description.Behaviors.Add(metadataBehavior);
            }
            // When set to True, adds the metadata exchange behavior.  
            if (value == true)
            {
               AddAllMexEndPoints();
            }
         }
      }

      public bool HasMexEndpoint
      {
         get
         {
            // TODO: Refactor...
            Predicate<ServiceEndpoint> mexEndPoint = delegate(ServiceEndpoint endpoint)
            {
               return endpoint.Contract.ContractType == typeof(IMetadataExchange);
            };
            return false; // TODO: Description.Endpoints.Contains(mexEndPoint);
         }
      }

      public void AddAllMexEndPoints()
      {
         Debug.Assert(HasMexEndpoint == false);
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
               case "net.http":
                  {
                     bindingElement = new HttpTransportBindingElement();
                     break;
                  }
               case "net.https":
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

      #endregion
   }
}
