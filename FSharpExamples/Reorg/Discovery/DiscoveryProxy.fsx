#r "System.Runtime.Serialization"
#r "System.ServiceModel"
#r "System.ServiceModel.Discovery"
open System
open System.Collections.Concurrent
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Discovery
open System.Runtime.Remoting.Messaging

[<ServiceContract>]
type IEcho =
    [<OperationContract>]
    abstract Echo : string -> string


type Echo() =
    interface IEcho with
        member this.Echo(s) =
            sprintf "You said, \"%s\"" s

type OnOnlineAnnouncementAsyncResult(callback, state) =
    inherit AsyncResult(callback, state)

    do this.Complete(true)

    static member End(result) =
        AsyncResult.End<OnOnlineAnnouncementAsyncResult>(result)


type OnOfflineAnnouncementAsyncResult(callback, state) =
    inherit: AsyncResult(callback, state)
    
    do this.Complete(true)

    public static void End(IAsyncResult result)
    {
        AsyncResult.End<OnOfflineAnnouncementAsyncResult>(result);
    }
}

sealed class OnFindAsyncResult : AsyncResult
{
    public OnFindAsyncResult(AsyncCallback callback, object state)
        : base(callback, state)
    {
        this.Complete(true);
    }
    
    public static void End(IAsyncResult result)
    {
        AsyncResult.End<OnFindAsyncResult>(result);
    }
}
    
sealed class OnResolveAsyncResult : AsyncResult
{
    EndpointDiscoveryMetadata matchingEndpoint;
    
    public OnResolveAsyncResult(EndpointDiscoveryMetadata matchingEndpoint, AsyncCallback callback, object state)
        : base(callback, state)
    {
        this.matchingEndpoint = matchingEndpoint;
        this.Complete(true);
    }
    
    public static EndpointDiscoveryMetadata End(IAsyncResult result)
    {
        OnResolveAsyncResult thisPtr = AsyncResult.End<OnResolveAsyncResult>(result);
        return thisPtr.matchingEndpoint;
    }
}




[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)>]
type DiscoveryProxyService() =
    inherit DiscoveryProxy()

    let onlineServices = new ConcurrentDictionary<EndpointAddress, EndpointDiscoveryMetadata>()

    member this.AddOnlineService(endpointDiscoveryMetadata : EndpointDiscoveryMetadata) =
        onlineServices.[endpointDiscoveryMetadata.Address] <- endpointDiscoveryMetadata

        printfn "Added service:"
        for contractName in endpointDiscoveryMetadata.ContractTypeNames do
            printfn "  %O" contractName

    member this.RemoveOnlineService(endpointDiscoveryMetadata : EndpointDiscoveryMetadata) =
        if endpointDiscoveryMetadata <> null then
            let result = ref endpointDiscoveryMetadata
            printfn "Removing service:"
            for contractName in endpointDiscoveryMetadata.ContractTypeNames do
                printfn "  %O" contractName

            onlineServices.TryRemove(endpointDiscoveryMetadata.Address, result) |> ignore

    member this.MatchFromOnlineService(findRequestContext : FindRequestContext) =
        for endpointDiscoveryMetadata in onlineServices.Values do
            if findRequestContext.Criteria.IsMatch(endpointDiscoveryMetadata)
                then findRequestContext.AddMatchingEndpoint(endpointDiscoveryMetadata)

    member this.MatchFromOnlineService(criteria : ResolveCriteria) =
        onlineServices.Values
        |> Seq.find (fun x -> criteria.Address = x.Address)


let rand() =
    (new Random()).Next(7000, 9999)

let guid() =
    Guid.NewGuid()

let address = sprintf "net.tcp://localhost:%O/%O" <| rand() <| guid()
let host = new ServiceHost(typeof<Echo>, new Uri(address))
host.AddDefaultEndpoints()
// Make Discoverable
host.Description.Behaviors.Add(new ServiceDiscoveryBehavior())
host.Description.Endpoints.Add(new UdpDiscoveryEndpoint())
host.Open()
printfn "Service listening on %O\n" address

let echoEndpoint = 
    new DynamicEndpoint(
        ContractDescription.GetContract(typeof<IEcho>),
        new NetTcpBinding())
let channelFactory = new ChannelFactory<IEcho>(echoEndpoint)
let echoChannel = channelFactory.CreateChannel()
printfn "Service found at %O\n" <| echoEndpoint.Address
printfn "%s" <| echoChannel.Echo("Discovery!")

(echoChannel :?> ICommunicationObject).Close()
host.Close()



