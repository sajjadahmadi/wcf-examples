﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DemoApplication.PerCallServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PerCallServiceReference.ICounterService")]
    public interface ICounterService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICounterService/IncrementAndReturnCount", ReplyAction="http://tempuri.org/ICounterService/IncrementAndReturnCountResponse")]
        int IncrementAndReturnCount();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ICounterServiceChannel : DemoApplication.PerCallServiceReference.ICounterService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class CounterServiceClient : System.ServiceModel.ClientBase<DemoApplication.PerCallServiceReference.ICounterService>, DemoApplication.PerCallServiceReference.ICounterService {
        
        public CounterServiceClient() {
        }
        
        public CounterServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CounterServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CounterServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CounterServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int IncrementAndReturnCount() {
            return base.Channel.IncrementAndReturnCount();
        }
    }
}