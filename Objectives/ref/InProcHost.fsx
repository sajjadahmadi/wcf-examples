#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
namespace Mcts_70_503
open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

type Endpoint = Type * Binding * string

type InProcHost<'THost>(uris: Uri[]) =
    let host = new ServiceHost(typeof<'THost>, uris)
    let endpoints = new List<Endpoint>()
    let mutable disposed = false
    
    let (|NetTcp|NetPipe|Http|Https|) (uri: Uri) =
        match uri.Scheme with
        | "net.tcp"  -> NetTcp
        | "net.pipe" -> NetPipe
        | "http"     -> Http
        | "https"    -> Https
        | _          -> failwith "unkown scheme"
    
    let getBaseAddress scheme =
        let tryScheme (u: Uri) =
            if u.Scheme = scheme
                then Some u
                else None
        host.BaseAddresses
        |> Seq.first tryScheme
    
    let addressForBinding (b: Binding) =
        match b with
        | :? NetTcpBinding       -> getBaseAddress "net.tcp"
        | :? NetNamedPipeBinding -> getBaseAddress "net.pipe"
        | :? BasicHttpBinding    -> getBaseAddress "http"
        | _                      -> failwith "unsupported binding"
    
    new() =
        new InProcHost<'THost>([| new Uri("net.pipe://localhost"); new Uri("http://localhost") |])
    
    member this.Open() =
        host.Open()
    
    member this.Close() =
        host.Close()
    
    member this.AddEndPoint<'TContract>() =
        this.AddEndPoint<'TContract>(new NetNamedPipeBinding())
    
    member this.AddEndPoint<'TContract>(binding: Binding) =
        this.AddEndPoint<'TContract>(binding, "")
        
    member this.AddEndPoint<'TContract>(binding: Binding, address: string) =
        match addressForBinding binding with
        | Some addr -> endpoints.Add((typeof<'TContract>, binding, addr.ToString()))
        | None      -> endpoints.Add((typeof<'TContract>, binding, ""))
        host.AddServiceEndpoint(typeof<'TContract>, binding, address) |> ignore
    
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