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


    type ExampleHost<'TService, 'TContract>(uri : string) as this =
        inherit ServiceHost(typeof<'TService>, new Uri(uri))
        
        let mutable proxies = []
        let binding = new NetTcpBinding()
        let addProxy p =
            proxies <- (box p :?> ICommunicationObject) :: proxies
            p
        do this.AddServiceEndpoint(typeof<'TContract>, binding, "") |> ignore
        
        new() = new ExampleHost<'TService, 'TContract>("net.tcp://localhost")
        
        member this.CreateProxy() =
            ChannelFactory<'TContract>.CreateChannel(binding, new EndpointAddress(string uri))
            |> addProxy
            
        override this.OnClosing() =
            proxies |> Seq.iter (fun x -> x.Close())
            base.OnClosing()
            
            
    let example<'TService, 'TContract> (f : 'TContract -> unit) =
        let host = new ExampleHost<'TService, 'TContract>()
        host.Open()
        let proxy = host.CreateProxy()
        f proxy
        host.Close()
        

    type PrintToConsoleMessageInspector() =
        interface IDispatchMessageInspector with
            member this.AfterReceiveRequest(request, channel, instanceContext) =
                printfn "========\nRequest\n========\n%A\n" request
                null
            
            member this.BeforeSendReply(reply, correlationState) =
                printfn "========\nReply\n========\n%A\n" reply


    type ApplyMessageInspectorBehavior(inspector : IDispatchMessageInspector) =
        interface IEndpointBehavior with
            member this.AddBindingParameters(endpoint, bindingParameters) = ()
            member this.ApplyClientBehavior(endpoint, clientRuntime) = ()
            member this.Validate(endpoint) = ()
            member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) =
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector)


    [<AttributeUsage(AttributeTargets.Class)>]
    type PrintMessagesToConsoleAttribute() =
        inherit Attribute()
        
        let inspector = new PrintToConsoleMessageInspector()
        let behavior = new ApplyMessageInspectorBehavior(inspector) :> IEndpointBehavior
        
        interface IServiceBehavior with
            member this.AddBindingParameters(description, host, endpoints, bindingParams) = ()
            member this.Validate(description, host) = ()
            member this.ApplyDispatchBehavior(description, host) =
                description.Endpoints
                |> Seq.iter (fun ep -> ep.Behaviors.Add(behavior))

