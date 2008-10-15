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

type HostEntry =
    { Type : Type; Host : ServiceHost; Binding : Binding; Uri : Uri } with
        override this.ToString() =
            this.Uri.ToString()

type InProcHost(binding, baseUri) =
    let hosts = new List<HostEntry>()
    
    let mutable disposed = false
    
    let entry tHost host binding uri =
        { Type = tHost; Host = host; Binding = binding; Uri = uri }
    
    let addHost he =
        hosts.Add(he)
        he
        
    let makeHost (tHost: Type, binding, uri: Uri) =
        let host = new ServiceHost(tHost, [| uri |])
        entry tHost host binding uri
    
    let hasHost (he: HostEntry) =
        hosts.Exists(fun x -> x.Equals(he))
        
    let addEndpoint he (tContract: Type) binding (uri: Uri) =
        printfn "Adding %s Endpoint:\n  %A\n  %A" tContract.Name binding uri
        he.Host.AddServiceEndpoint(tContract, binding, uri) |> ignore
        he.Host.Open()
    
    [<OverloadID("new.0")>]
    new() = new InProcHost("net.pipe://localhost")
    
    [<OverloadID("new.1")>]
    new(baseAddress: string) =
        let binding = new NetNamedPipeBinding(TransactionFlow = true) :> Binding
        new InProcHost(binding, baseAddress)
    
    [<OverloadID("new.2")>]
    new(binding: Binding, baseAddress: string) =
        let uri = new Uri(baseAddress)
        new InProcHost(binding, uri)

    member private this.FindHostEntry(f) =
        hosts |>
        Seq.tryfind f

    member private this.GetHostEntry<'THost,'TContract>(?bnd, ?uri) =
        let addept he =
            addEndpoint he (typeof<'TContract>) he.Binding he.Uri
            he
        match bnd,uri with
        | None,None     ->
            match this.FindHostEntry(fun x -> x.Type = typeof<'THost>) with
            | None    -> makeHost(typeof<'THost>, binding, baseUri) |> addHost |> addept
            | Some he -> he    
        | Some b,None   ->
            match this.FindHostEntry(fun x -> x.Type = typeof<'THost> && x.Binding = b) with
            | None    -> makeHost(typeof<'THost>, b, baseUri) |> addHost |> addept
            | Some he -> he
        | None,Some u   ->
            match this.FindHostEntry(fun x -> x.Type = typeof<'THost> && x.Uri = u) with
            | None    -> makeHost(typeof<'THost>, binding, u) |> addHost |> addept
            | Some he -> he
        | Some b,Some u ->
            match this.FindHostEntry(fun x -> x.Type = typeof<'THost> && x.Binding = b && x.Uri = u) with
            | None    -> makeHost(typeof<'THost>, b, u) |> addHost |> addept
            | Some he -> he

    [<OverloadID("CreateProxy.0")>]
    member this.CreateProxy<'THost,'TContract when 'THost : not struct and 'TContract : not struct>() =
        let he = this.GetHostEntry<'THost,'TContract>()
        this.CreateProxy<'TContract>(he)

    [<OverloadID("CreateProxy.1")>]
    member this.CreateProxy<'THost,'TContract when 'THost : not struct and 'TContract : not struct>(relativeAddress: string) =
        let aburi = new Uri(baseUri, relativeAddress)
        let he = this.GetHostEntry<'THost,'TContract>(uri=aburi)
        this.CreateProxy<'TContract>(he)
        
    [<OverloadID("CreateProxy.2")>]
    member this.CreateProxy<'THost,'TContract when 'THost : not struct and 'TContract : not struct>(binding, baseAddress: string) =
        this.CreateProxy<'THost,'TContract>(binding, new Uri(baseAddress))
    
    [<OverloadID("CreateProxy.3")>]
    member this.CreateProxy<'THost,'TContract when 'THost : not struct and 'TContract : not struct>(binding, uri) =
        let he = this.GetHostEntry<'THost,'TContract>(binding, uri)
        this.CreateProxy<'TContract>(he)
    
    [<OverloadID("CreateProxy.4")>]
    member private this.CreateProxy<'TContract when 'TContract : not struct>(he) =
        ChannelFactory<'TContract>.CreateChannel(he.Binding, new EndpointAddress(he.ToString()))
    
    member this.CloseProxy<'TContract when 'TContract : not struct> (i: 'TContract) =
        let i = box i
        match i with
        | :? ICommunicationObject -> (i :?> ICommunicationObject).Close()
        | _ -> ()
    
    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                for he in hosts do
                    he.Host.Close()
                disposed <- true

// Example
//[<ServiceContract>]
//type IMyContract =
//    [<OperationContract>]
//    abstract MyOperation : unit -> string
//
//[<ServiceContract>]
//type IMyContract2 =
//    [<OperationContract>]
//    abstract MyOperation : unit -> string
//
//type MyService() =
//    interface IMyContract with
//        member this.MyOperation() = "My Message"
//    interface IMyContract2 with
//        member this.MyOperation() = "My Other Message"
//        
//let host = new InProcHost()
//
//let proxy1 = host.CreateProxy<MyService, IMyContract>()
//let result1 = proxy1.MyOperation()
//host.CloseProxy(proxy1)
//printfn "%s" result1
//
//let proxy2 = host.CreateProxy<MyService, IMyContract>()
//let result2 = proxy2.MyOperation()
//host.CloseProxy(proxy2)
//printfn "%s" result2
//
//let proxy3 = host.CreateProxy<MyService, IMyContract2>("service2")
//let result3 = proxy3.MyOperation()
//host.CloseProxy(proxy3)
//printfn "%s" result3
//
//let proxy4 = host.CreateProxy<MyService, IMyContract>(new BasicHttpBinding(), new Uri("http://localhost"))
//let result4 = proxy4.MyOperation()
//host.CloseProxy(proxy4)
//printfn "%s" result4
//
//(host :> IDisposable).Dispose()
