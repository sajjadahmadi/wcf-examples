#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
namespace Mcts_70_503
open System
open System.Diagnostics
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

type InProcHost(binding: Binding, baseUri: Uri) =
    let hosts = new Dictionary<Type, ServiceHost * Uri>()
    
    let mutable disposed = false
    
    let addHost (ts: Type) (ti: Type) binding (uri: Uri) =
        let host = new ServiceHost(ts, [| uri |])
        host.AddServiceEndpoint(ti, binding, uri) |> ignore
        host.Open()
        printfn "Adding %A host:\n  %A\n  %A" ti.Name binding uri
        let newHost = host, uri
        hosts.Add(ti, newHost)
        newHost
    
    let getHost ti =
        if hosts.ContainsKey(ti)
            then Some hosts.[ti]
            else None
    
    let getUri (ti: Type) =
        match getHost ti with
        | Some (_,uri) -> uri
        | None         -> new Uri(baseUri, Guid.NewGuid().ToString())
    
    [<OverloadID("new.0")>]
    new() = new InProcHost("net.pipe://localhost")
    
    [<OverloadID("new.1")>]
    new(baseAddress: string) =
        let binding = new NetNamedPipeBinding(TransactionFlow = true)
        new InProcHost(binding, baseAddress)
    
    [<OverloadID("new.2")>]
    new(binding: Binding, baseAddress: string) =
        let uri = new Uri(baseAddress)
        new InProcHost(binding, uri)

    [<OverloadID("Service.0")>]
    member this.Service<'S,'I when 'S : not struct and 'I : not struct>() =
        let uri = getUri (typeof<'I>)
        this.Service<'S,'I>(binding, uri)
    
    [<OverloadID("Service.1")>]
    member this.Service<'S,'I when 'S : not struct and 'I : not struct>(binding: Binding, baseAddress: string) =
        let uri = new Uri(baseAddress)
        this.Service<'S,'I>(binding, uri)
    
    [<OverloadID("Service.2")>]
    member this.Service<'S,'I when 'S : not struct and 'I : not struct>(binding: Binding, uri: Uri) =
        let ts = typeof<'S>
        let ti = typeof<'I>
        let host,storedUri =
            match getHost ti with
            | Some (x,y) -> x,y
            | None       -> addHost ts ti binding uri
        ChannelFactory<'I>.CreateChannel(binding, new EndpointAddress(storedUri.ToString()))
    
    member this.CloseService<'I when 'I : not struct> (i: 'I) =
        if hosts.Remove(typeof<'I>) then
            let i = box i
            match i with
            | :? ICommunicationObject -> (i :?> ICommunicationObject).Close()
            | _ -> ()
    
    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                for kv in hosts do
                    let host,_ = kv.Value
                    host.Close()
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
//let host = new InProcHost()
//let proxy1 = host.Service<MyService, IMyContract>()
//let result1 = proxy1.MyOperation()
//printfn "%s" result1
//
//let proxy2 = host.Service<MyService, IMyContract>()
//let result2 = proxy2.MyOperation()
//printfn "%s" result2
//
//host.CloseService(proxy1)
//host.CloseService(proxy2)
//
//let proxy3 = host.Service<MyService, IMyContract>(new BasicHttpBinding(), new Uri("http://localhost"))
//let result3 = proxy3.MyOperation()
//printfn "%s" result3
//
//host.CloseService(proxy3)
//
//(host :> IDisposable).Dispose()
