﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="http://tempuri.org", ConfigurationName="MyService")]
public interface MyService
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/MyService/GetHeader", ReplyAction="http://tempuri.org/MyService/GetHeaderResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(string), Action="http://tempuri.org/MyService/GetHeaderStringFault", Name="string", Namespace="http://schemas.microsoft.com/2003/10/Serialization/")]
    string GetHeader(string name, string ns);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface MyServiceChannel : MyService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class MyServiceClient : System.ServiceModel.ClientBase<MyService>, MyService
{
    
    public MyServiceClient()
    {
    }
    
    public MyServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public MyServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public MyServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public MyServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public string GetHeader(string name, string ns)
    {
        return base.Channel.GetHeader(name, ns);
    }
}