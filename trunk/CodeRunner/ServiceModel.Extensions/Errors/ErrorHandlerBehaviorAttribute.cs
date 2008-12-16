using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Reflection;

namespace System.ServiceModel.Errors
{
    public class ErrorHandlerBehaviorAttribute : Attribute, IErrorHandlerBehavior
    {
        IErrorHandlerBehavior behavior;

        public ErrorHandlerBehaviorAttribute(Type behaviorType)
        {
            behavior = Activator.CreateInstance(behaviorType) as IErrorHandlerBehavior ;
        }

        #region IErrorHandler Members

        public bool HandleError(Exception error)
        { 
            return behavior.HandleError(error); 
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        { 
            behavior.ProvideFault(error, version, ref fault); 
        }

        #endregion

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription description, ServiceHostBase host, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { behavior.AddBindingParameters(description, host, endpoints, bindingParameters); }
        public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        { behavior.ApplyDispatchBehavior(description, host); }
        public void Validate(ServiceDescription description, ServiceHostBase host)
        { behavior.Validate(description, host); }

        #endregion

        #region IErrorHandlerBehavior Members

        public Type ServiceType
        {
            get { return behavior.ServiceType; }
        }

        #endregion
    }
}