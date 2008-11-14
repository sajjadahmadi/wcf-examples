﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client.MyServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="FaultType", Namespace="http://schemas.datacontract.org/2004/07/Faults")]
    [System.SerializableAttribute()]
    public partial class FaultType : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MyServiceReference.IMyContract")]
    public interface IMyContract {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMyContract/ThrowTypedFault", ReplyAction="http://tempuri.org/IMyContract/ThrowTypedFaultResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Client.MyServiceReference.FaultType), Action="http://tempuri.org/IMyContract/ThrowTypedFaultFaultTypeFault", Name="FaultType", Namespace="http://schemas.datacontract.org/2004/07/Faults")]
        void ThrowTypedFault();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMyContract/ThrowUntypedFault", ReplyAction="http://tempuri.org/IMyContract/ThrowUntypedFaultResponse")]
        void ThrowUntypedFault();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IMyContractChannel : Client.MyServiceReference.IMyContract, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class MyContractClient : System.ServiceModel.ClientBase<Client.MyServiceReference.IMyContract>, Client.MyServiceReference.IMyContract {
        
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
        
        public void ThrowTypedFault() {
            base.Channel.ThrowTypedFault();
        }
        
        public void ThrowUntypedFault() {
            base.Channel.ThrowUntypedFault();
        }
    }
}