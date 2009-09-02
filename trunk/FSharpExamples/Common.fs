[<assembly: AutoOpen("Common.Common")>] do()

module Common =
    open System
    open System.IO
    open System.Xml.Linq
    open System.Runtime.Serialization
    open System.ServiceModel
    open System.ServiceModel.Channels
    open System.ServiceModel.Description
    open System.ServiceModel.Dispatcher

    let serialize<'a> (x: 'a) =
        let serializer = new DataContractSerializer(typeof<'a>)
        let stream = new MemoryStream()
        serializer.WriteObject(stream, x)
        stream.Position <- 0L
        let reader = new StreamReader(stream)
        let doc = XDocument.Parse(reader.ReadToEnd())
        doc.ToString()

    let deserialize<'a> (x: string) =
        let stream = new MemoryStream()
        let data = System.Text.Encoding.UTF8.GetBytes(x)
        stream.Write(data, 0, data.Length)
        stream.Position <- 0L
        let deserializer = new DataContractSerializer(typeof<'a>)
        deserializer.ReadObject(stream) :?> 'a
    

    type ExampleHost<'TService, 'TContract>(binding : Binding, [<ParamArray>] uris) as this =
        inherit ServiceHost(typeof<'TService>, uris)
        
        let mutable proxies = []
        let addProxy p =
            proxies <- (box p :?> ICommunicationObject) :: proxies
            p
        do this.AddServiceEndpoint(typeof<'TContract>, binding, "") |> ignore
        
        new() = 
            new ExampleHost<'TService, 'TContract>(new NetTcpBinding(), "net.tcp://localhost")
            
        [<OverloadID("ctor1")>]
        new(binding : Binding, [<ParamArray>] uris : string[]) = 
            let uris = uris |> Array.map (fun uri -> new Uri(uri))
            new ExampleHost<'TService, 'TContract>(binding, uris)
        
        member this.CreateProxyOf<'T>() =
            ChannelFactory<'T>.CreateChannel(binding, new EndpointAddress(uris.[0]))
            |> addProxy

        member this.CreateProxy() =
            ChannelFactory<'TContract>.CreateChannel(binding, new EndpointAddress(uris.[0]))
            |> addProxy
        
        member this.CreateChannelFactory() =
            new ChannelFactory<'TContract>(binding, new EndpointAddress(uris.[0]))
        
        member this.EnableHttpGet() =
            let debugBehavior = this.Description.Behaviors.Find<ServiceMetadataBehavior>()
            if debugBehavior = null
                then this.Description.Behaviors.Add(new ServiceMetadataBehavior(HttpGetEnabled = true))
                else debugBehavior.HttpGetEnabled <- true
        
        member this.IncludeExceptionDetails() =
            let mutable behavior = this.Description.Behaviors.Find<ServiceDebugBehavior>()
            if behavior = null then
                behavior <- new ServiceDebugBehavior()
                this.Description.Behaviors.Add(behavior)
            behavior.IncludeExceptionDetailInFaults <- true
                        
        override this.OnClosing() =
            proxies |> Seq.iter (fun x -> x.Close())
            base.OnClosing()
            

    let example2<'TService, 'TContract> (fini : unit -> ExampleHost<'TService, 'TContract>) (f : ExampleHost<'TService, 'TContract> -> 'TContract -> unit) =
        let host = fini()
        host.Open()
        let proxy = host.CreateProxy()
        f host proxy
        host.Close()
        
    let example<'TService, 'TContract> (f : ExampleHost<'TService, 'TContract> -> 'TContract -> unit) =
        let fini() = new ExampleHost<'TService, 'TContract>()
        example2<'TService, 'TContract> fini f
        

    type PrintToConsoleMessageInspector() =
        interface IDispatchMessageInspector with
            member this.AfterReceiveRequest(request, channel, instanceContext) =
                printfn "========\nRequest\n========\n%A\n" request
                null
            
            member this.BeforeSendReply(reply, correlationState) =
                printfn "========\nReply\n========\n%A\n" reply


    type ApplyDispatchMessageInspectorBehavior(inspector : IDispatchMessageInspector) =
        interface IEndpointBehavior with
            member this.AddBindingParameters(endpoint, bindingParameters) = ()
            member this.ApplyClientBehavior(endpoint, clientRuntime) = ()
            member this.Validate(endpoint) = ()
            member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) =
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector)


    type ApplyClientMessageInspectorBehavior(inspector : IClientMessageInspector) =
        interface IEndpointBehavior with
            member this.AddBindingParameters(endpoint, bindingParameters) = ()
            member this.Validate(endpoint) = ()
            member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) = ()
            member this.ApplyClientBehavior(endpoint, clientRuntime) =
                clientRuntime.MessageInspectors.Add(inspector)                
                

    [<AttributeUsage(AttributeTargets.Class)>]
    type PrintMessagesToConsoleAttribute() =
        inherit Attribute()
        
        let inspector = new PrintToConsoleMessageInspector()
        let behavior = new ApplyDispatchMessageInspectorBehavior(inspector) :> IEndpointBehavior
        
        interface IServiceBehavior with
            member this.AddBindingParameters(description, host, endpoints, bindingParams) = ()
            member this.Validate(description, host) = ()
            member this.ApplyDispatchBehavior(description, host) =
                description.Endpoints
                |> Seq.iter (fun ep -> ep.Behaviors.Add(behavior))

