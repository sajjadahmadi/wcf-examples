#r "System.Runtime.Serialization"
#r "System.ServiceModel"
#r "System.ServiceModel.Discovery"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Discovery


[<ServiceContract>]
type IEcho =
    [<OperationContract>]
    abstract Echo : string -> string


type Echo() =
    interface IEcho with
        member this.Echo(s) =
            sprintf "You said, \"%s\"" s


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


let discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint())
let echoServices = discoveryClient.Find(new FindCriteria(typeof<IEcho>))
discoveryClient.Close()

let echoEndpoint = echoServices.Endpoints.[0]

let channelFactory = new ChannelFactory<IEcho>(new NetTcpBinding(), echoEndpoint.Address)
let echoChannel = channelFactory.CreateChannel()
printfn "Service found at %O\n" <| echoEndpoint.Address
printfn "%s" <| echoChannel.Echo("Discovery!")

(echoChannel :?> ICommunicationObject).Close()
host.Close()



