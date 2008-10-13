#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Description

type private HostRecord = { Host : ServiceHost; Address : string }

type InProcFactory(uri: Uri) =
    let hosts = new Dictionary<Type, HostRecord>()
    
    let mutable binding = new NetNamedPipeBinding(TransactionFlow = true)
    
    let mutable started = false
    
    do AppDomain.CurrentDomain.ProcessExit.Add(fun e ->
        for pair in hosts do
            pair.Value.Host.Close())

    new() = InProcFactory("net.pipe://localhost")
    
    [<OverloadID("new.string")>]
    new(address: string) =
        let uri = new Uri(address)
        InProcFactory(uri)

    member this.Binding with get() = binding
                        and set v =
                            if hosts.Count > 0
                                then binding <- v
                                else failwith "Cannot change binding while instances exist."
                        
    member private this.GetHostRecord<'S,'I>() =
        let ts = typeof<'S>
        if hosts.ContainsKey(ts)
            then hosts.[ts]
            else
                let host = new ServiceHost(ts, [| uri |])
                let address = sprintf "%A%A" uri (Guid.NewGuid())
                let result = { Host = host; Address = address}
                hosts.Add(ts, result)
                host.AddServiceEndpoint(typeof<'I>, binding, address) |> ignore
                host.Open()
                result

    member this.CreateInstance<'S,'I when 'S : not struct and 'I : not struct>() =
        let hostRecord = this.GetHostRecord<'S,'I>()
        ChannelFactory<'I>.CreateChannel(binding, new EndpointAddress(hostRecord.Address))

    member this.CloseInstance<'I when 'I : not struct> (i: 'I) =
        if hosts.Remove(i) then
            let i = box i
            match i with
            | :? ICommunicationObject -> (i :?> ICommunicationObject).Close()
            | _ -> ()
        
        
//// Example
//[<ServiceContract>]
//type IMyContract =
//    [<OperationContract>]
//    abstract MyOperation : unit -> string
//
//type MyService() =
//    interface IMyContract with
//        member this.MyOperation() = "My Message"
//
//let fact = InProcFactory()
//let proxy1 = fact.CreateInstance<MyService, IMyContract>()
//let result1 = proxy1.MyOperation()
//printfn "%s" result1
//
//let proxy2 = fact.CreateInstance<MyService, IMyContract>()
//let result2 = proxy2.MyOperation()
//printfn "%s" result2
//
//fact.CloseInstance(proxy1)
//fact.CloseInstance(proxy2)
