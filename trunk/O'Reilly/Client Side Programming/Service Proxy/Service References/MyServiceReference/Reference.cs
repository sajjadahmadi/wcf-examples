﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServiceProxy.MyServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MyServiceReference.IMyContract")]
    public interface IMyContract {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMyContract/MyOperation", ReplyAction="http://tempuri.org/IMyContract/MyOperationResponse")]
        string MyOperation();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IMyContractChannel : ServiceProxy.MyServiceReference.IMyContract, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class MyContractClient : System.ServiceModel.ClientBase<ServiceProxy.MyServiceReference.IMyContract>, ServiceProxy.MyServiceReference.IMyContract {
        
        public MyContractClient() {
        }
        
        public MyContractClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MyContractClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MyContractClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MyContractClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string MyOperation() {
            return base.Channel.MyOperation();
        }
    }
}
