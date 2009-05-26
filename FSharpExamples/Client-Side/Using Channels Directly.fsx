#r "System.Runtime.Serialization"
#r "System.ServiceModel"
open System
open System.ServiceModel
open System.ServiceModel.Description


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let channel = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

let result = channel.MyOperation()
printfn "Result: %s" result

(channel :?> ICommunicationObject).Close()
host.Close()
