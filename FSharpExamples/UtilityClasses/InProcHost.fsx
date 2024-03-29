#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

type Endpoint = Type * Binding * string

[<AutoOpen>]
module internal InProcHost =
    let defaultBaseAddresses =
        [| new Uri("net.pipe://localhost"); 
           new Uri("net.tcp://localhost");
           new Uri("http://localhost") |]

    let (|NetTcp|NetPipe|Http|Https|) (uri: Uri) =
        match uri.Scheme with
        | "net.tcp"  -> NetTcp
        | "net.pipe" -> NetPipe
        | "http"     -> Http
        | "https"    -> Https
        | _          -> failwith "unknown scheme"
        
    let getBaseUri (host: ServiceHost) scheme =
        let tryScheme (u: Uri) =
            if u.Scheme = scheme
                then Some u
                else None
        host.BaseAddresses
        |> Seq.first tryScheme

    let getBaseUriForBinding (host: ServiceHost) (b: Binding) =
        match b with
        | :? NetTcpBinding       -> getBaseUri host "net.tcp"
        | :? NetNamedPipeBinding -> getBaseUri host "net.pipe"
        | :? BasicHttpBinding  |
          :? WSHttpBinding     |
          :? WSDualHttpBinding   -> getBaseUri host "http"
        | _                      -> failwith "unsupported binding"
    
type InProcHost<'THost>(host: ServiceHost) =
    let endpoints = new List<Endpoint>()
    let mutable disposed = false
    
    [<OverloadID("ctor.1")>]
    new() =
        new InProcHost<'THost>(InProcHost.defaultBaseAddresses)
    
    [<OverloadID("ctor.2")>]
    new(uris: Uri[]) =
        let h = new ServiceHost(typeof<'THost>, uris)
        new InProcHost<'THost>(h)
    
    [<OverloadID("ctor.3")>]
    new(singletonInstance: 'THost) =
        new InProcHost<'THost>(singletonInstance, InProcHost.defaultBaseAddresses)
        
    [<OverloadID("ctor.4")>]
    new(singletonInstance: 'THost, uris) =
        let h = new ServiceHost((singletonInstance :> obj), uris)
        new InProcHost<'THost>(h)
    
    member this.InnerHost = host

    member this.IncludeExceptionDetailInFaults
        with get() =
            let debuggingBehavior = host.Description.Behaviors.Find<ServiceBehaviorAttribute>()
            debuggingBehavior.IncludeExceptionDetailInFaults
        and set v =
            if host.State = CommunicationState.Opened then
                raise (new InvalidOperationException("Host is already opened"))
            let debuggingBehavior = host.Description.Behaviors.Find<ServiceBehaviorAttribute>()
            debuggingBehavior.IncludeExceptionDetailInFaults <- v
            
    member this.Open() =
        host.Open()
    
    member this.Close() =
        host.Close()
    
    member this.AddEndpoint<'TContract>() =
        this.AddEndpoint<'TContract>(new NetNamedPipeBinding())
    
    member this.AddEndpoint<'TContract>(binding: Binding) =
        this.AddEndpoint<'TContract>(binding, "")
        
    member this.AddEndpoint<'TContract>(binding: Binding, relativeAddress: string) =
        let baseUri = getBaseUriForBinding host binding
        let absAddress =
            match baseUri with
            | Some uri -> (new Uri(uri, relativeAddress)).ToString()
            | None     -> failwith "there is no base address for specified binding"
        endpoints.Add((typeof<'TContract>, binding, absAddress))
        host.AddServiceEndpoint(typeof<'TContract>, binding, absAddress) |> ignore
    
    member this.EnableMetadataExchange() =
        let add (el: BindingElement) =
            let binding = new CustomBinding([| el |])
            host.AddServiceEndpoint(typeof<IMetadataExchange>, binding, "mex") |> ignore

        let behavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>()
        if behavior = null then
            let mexBehavior = new ServiceMetadataBehavior()
            mexBehavior.HttpGetEnabled <- true
            host.Description.Behaviors.Add(mexBehavior)
            
            for uri in host.BaseAddresses do
                match uri with
                | NetTcp  -> add (new TcpTransportBindingElement())
                | NetPipe -> add (new NamedPipeTransportBindingElement())
                | Http    -> add (new HttpTransportBindingElement())
                | Https   -> add (new HttpsTransportBindingElement())
    
    [<OverloadID("CreateProxy.1")>]
    member this.CreateProxy<'TContract>() =
        let f ((t,_,_) as x) =
            if t = typeof<'TContract>
                then Some x
                else None
        this.CreateProxy<'TContract>(f)
        
    [<OverloadID("CreateProxy.2")>]
    member this.CreateProxy<'TContract, 'TBinding>() =
        let f ((t,b,_) as x) =
            if t = typeof<'TContract> && b.GetType() = typeof<'TBinding>
                then Some x
                else None
        this.CreateProxy<'TContract>(f)
    
    member private this.CreateProxy<'TContract>(f) =
        if endpoints.Count = 0
            then failwith "host has no endpoints"
        let first = 
            endpoints
            |> Seq.first f
        match first with
        | None         -> failwith "host has no endpoints of that contract and/or binding type"
        | Some (_,b,a) ->
            ChannelFactory<'TContract>.CreateChannel(b, new EndpointAddress(a))

    member this.CloseProxy(instance) =
        let instance = box instance
        match instance with
        | :? ICommunicationObject ->
            let co = instance :?> ICommunicationObject
            co.Close()
        | _ -> ()

    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                this.Close()
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
//let host = new InProcHost<MyService>([| new Uri("net.pipe://localhost"); new Uri("http://localhost") |])
//host.AddEndPoint<IMyContract>(new NetNamedPipeBinding())
//let binding = new BasicHttpBinding()
//host.AddEndPoint<IMyContract>(binding)
//host.AddEndPoint<IMyContract2>(binding)
//host.EnableMetadataExchange()
//host.Open()
//
//let p = host.CreateProxy<IMyContract>()
//printfn "%s" (p.MyOperation())
//host.CloseProxy(p)
//
//System.Console.ReadKey(true)
//
//host.Close()