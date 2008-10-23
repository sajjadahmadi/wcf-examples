#light
// This example is built to run against the "Self Hosting" Example
#r "System.Runtime.Serialization"
#r "System.ServiceModel"
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

let binding = new WSHttpBinding() :> Binding
let addr = new EndpointAddress("http://localhost:8000/MyService")
let channel = ChannelFactory<IMyContract>.CreateChannel(binding, addr)

let result = channel.MyOperation()
printfn "Result: %s" result
