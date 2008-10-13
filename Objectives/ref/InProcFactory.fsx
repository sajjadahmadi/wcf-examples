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
    
    let mutable disposed = false
    
    do AppDomain.CurrentDomain.ProcessExit.Add(fun e ->
        for pair in hosts do
            pair.Value.Host.Close())

    new() = new InProcFactory("net.pipe://localhost")
    
    [<OverloadID("new.string")>]
    new(address: string) =
        let uri = new Uri(address)
        new InProcFactory(uri)

    member this.Binding with get() = binding
                        and set v =
                            if hosts.Count > 0
                                then binding <- v
                                else failwith "Cannot change binding while instances exist."
                        
    member private this.GetHostRecord<'S,'I>() =
        let ts = typeof<'S>
        let ti = typeof<'I>
        if hosts.ContainsKey(ti)
            then hosts.[ti]
            else
                let host = new ServiceHost(ts, [| uri |])
                let address = sprintf "%A%A" uri (Guid.NewGuid())
                let result = { Host = host; Address = address}
                hosts.Add(ti, result)
                host.AddServiceEndpoint(ti, binding, address) |> ignore
                host.Open()
                result

    member this.GetInstance<'S,'I when 'S : not struct and 'I : not struct>() =
        let hostRecord = this.GetHostRecord<'S,'I>()
        ChannelFactory<'I>.CreateChannel(binding, new EndpointAddress(hostRecord.Address))

    member this.CloseInstance<'I when 'I : not struct> (i: 'I) =
        if hosts.Remove(typeof<'I>) then
            let i = box i
            match i with
            | :? ICommunicationObject -> (i :?> ICommunicationObject).Close()
            | _ -> ()
    
    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                for host in hosts do
                    host.Value.Host.Close()
                disposed <- true
    
    
// Example
//[<ServiceContract>]
//type IMyContract =
//    [<OperationContract>]
//    abstract MyOperation : unit -> string
//
//type MyService() =
//    interface IMyContract with
//        member this.MyOperation() = "My Message"
//
//let fact = new InProcFactory()
//let proxy1 = fact.GetInstance<MyService, IMyContract>()
//let result1 = proxy1.MyOperation()
//printfn "%s" result1
//
//let proxy2 = fact.GetInstance<MyService, IMyContract>()
//let result2 = proxy2.MyOperation()
//printfn "%s" result2
//
//fact.CloseInstance(proxy1)
//fact.CloseInstance(proxy2)
//(fact :> IDisposable).Dispose()
