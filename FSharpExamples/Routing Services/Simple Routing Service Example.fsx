#r "System.Runtime.Serialization"
#r "System.ServiceModel"
#r "System.ServiceModel.Routing"
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher
open System.ServiceModel.Routing


[<ServiceContract>]
type IHelloService =
    [<OperationContract>]
    abstract SayHello : unit -> string


type HelloService() =
    interface IHelloService with
        member this.SayHello() = printfn "Hello Service call..."; "Hello!"


let clientAddress = "http://localhost:8000/service"
let routerAddress = "http://localhost:8000/router"

let routerBinding = new WSHttpBinding()
let clientBinding = new WSHttpBinding()

let routerHost = new ServiceHost(typeof<RoutingService>)
let helloHost = new ServiceHost(typeof<HelloService>)

//add the endpoint the router will use to recieve messages
routerHost.AddServiceEndpoint(typeof<IRequestReplyRouter>, routerBinding, routerAddress)

helloHost.AddServiceEndpoint(typeof<IHelloService>, clientBinding, clientAddress)

//create the client endpoint the router will route messages to
let contract = ContractDescription.GetContract(typeof<IRequestReplyRouter>)
let routerClient = new ServiceEndpoint(contract, clientBinding, new EndpointAddress(clientAddress))

//create a new routing configuration object
let rc = new RoutingConfiguration()

//create the endpoint list that contains the service endpoints we want to route to
//in this case we have only one
let endpointList = new List<ServiceEndpoint>()
endpointList.Add(routerClient)

//add a MatchAll filter to the Router's filter table
//map it to the endpoint list defined earlier
//when a message matches this filter, it will be sent to the endpoint contained in the list
rc.FilterTable.Add(new MatchAllMessageFilter(), endpointList)

//attach the behavior to the service host
routerHost.Description.Behaviors.Add(new RoutingBehavior(rc))

helloHost.Open()
routerHost.Open()

let client = ChannelFactory<IHelloService>.CreateChannel(routerBinding, new EndpointAddress(routerAddress))
let result = client.SayHello()
printfn "RESULT: %s" result
(client :?> ICommunicationObject).Close()

helloHost.Close()
routerHost.Close()

