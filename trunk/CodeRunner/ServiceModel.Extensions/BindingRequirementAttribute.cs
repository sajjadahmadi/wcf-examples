using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BindingRequirementAttribute : Attribute, IServiceBehavior, IEndpointBehavior
    {
        public bool TransactionFlowRequired { get; set; }

        #region IServiceBehavior Members
        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase host, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        { }
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        { }
        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase host)
        {
            if (TransactionFlowRequired == true)
            {
                IEndpointBehavior behavior = this;
                foreach (ServiceEndpoint endpoint in description.Endpoints)
                { behavior.Validate(endpoint); }
            }
        }
        #endregion

        #region IEndpointBehavior Members
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        { }
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        { }
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        { }
        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
            if (TransactionFlowRequired)
            { ValidateTransactionFlow(endpoint); }
        }
        #endregion

        #region TransactionFlowRequirement
        static void ValidateTransactionFlow(ServiceEndpoint endpoint)
        {
            Exception exception = new InvalidOperationException("BindingRequirementAttribute requires transaction flow enabled, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " has it disabled");

            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                foreach (IOperationBehavior behavior in operation.Behaviors)
                {
                    if (behavior is TransactionFlowAttribute)
                    {
                        TransactionFlowAttribute attribute = behavior as TransactionFlowAttribute;
                        if (attribute.Transactions == TransactionFlowOption.Allowed)
                        {
                            if (endpoint.Binding is NetTcpBinding)
                            {
                                NetTcpBinding tcpBinding = endpoint.Binding as NetTcpBinding;
                                if (tcpBinding.TransactionFlow == false)
                                {
                                    throw exception;
                                }
                                break;
                            }
                            if (endpoint.Binding is NetNamedPipeBinding)
                            {
                                NetNamedPipeBinding ipcBinding = endpoint.Binding as NetNamedPipeBinding;
                                if (ipcBinding.TransactionFlow == false)
                                {
                                    throw exception;
                                }
                                break;
                            }
                            if (endpoint.Binding is WSHttpBindingBase)
                            {
                                WSHttpBindingBase wsBinding = endpoint.Binding as WSHttpBindingBase;
                                if (wsBinding.TransactionFlow == false)
                                {
                                    throw exception;
                                }
                                break;
                            }
                            if (endpoint.Binding is WSDualHttpBinding)
                            {
                                WSDualHttpBinding wsDualBinding = endpoint.Binding as WSDualHttpBinding;
                                if (wsDualBinding.TransactionFlow == false)
                                {
                                    throw exception;
                                }
                                break;
                            }
                            throw new InvalidOperationException("BindingRequirementAttribute requires transaction flow enabled, but binding for the endpoint with contract " + endpoint.Contract.ContractType + " does not support transaction flow");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
