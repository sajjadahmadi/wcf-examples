#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Description

type HostRecord = { Host : ServiceHost; Address : string }

module InProcFactory =
    
    let baseAddress = new Uri("net.pipe://localhost/")
    
    let hosts = new Dictionary<Type, HostRecord>()

    let binding = new NetNamedPipeBinding(TransactionFlow = true) 
   
    do AppDomain.CurrentDomain.ProcessExit.Add(fun e ->
        for pair in hosts do
            pair.Value.Host.Close())
    
    let getHostRecord<'S,'I when 'S : not struct and 'I : not struct> =
        if hosts.ContainsKey(typeof<'S>)
            then hosts.[typeof<'S>]
            else
                let host = new ServiceHost(typeof<'S>, [| baseAddress |])
                let address = sprintf "%A%A" baseAddress (Guid.NewGuid())
                let result = { Host = host; Address = address}
                hosts.Add(typeof<'S>, result)
                host.AddServiceEndpoint(typeof<'I>, binding, address) |> ignore
                host.Open()
                result  
    
    let createInstance<'S,'I when 'S : not struct and 'I : not struct> =
        let hostRecord = getHostRecord<'S,'I>
        ChannelFactory<'I>.CreateChannel(binding, new EndpointAddress(hostRecord.Address))

    let closeInstance<'I when 'I : not struct> (i: 'I) =
        let i = box i
        match i with
        | :? ICommunicationObject -> (i :?> ICommunicationObject).Close()
        | _ -> ()


// Example
[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"

let proxy1 = InProcFactory.createInstance<MyService, IMyContract>
let result1 = proxy1.MyOperation()
printfn "%s" result1

let proxy2 = InProcFactory.createInstance<MyService, IMyContract>
let result2 = proxy2.MyOperation()
printfn "%s" result2

InProcFactory.closeInstance(proxy1)
InProcFactory.closeInstance(proxy2)
