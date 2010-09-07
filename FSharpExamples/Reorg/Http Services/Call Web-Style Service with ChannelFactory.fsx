#r "System.ServiceModel"
#r "System.ServiceModel.Web"
#load "_Service.fsx"
open System.ServiceModel
open System.ServiceModel.Description

let factory = new ChannelFactory<Service.IService>(new WebHttpBinding(), "http://localhost:8000")
factory.Endpoint.Behaviors.Add(new WebHttpBehavior())
let channel = factory.CreateChannel()

printfn "Calling EchoWithGet via HTTP GET: "
let sGet = channel.EchoWithGet("Hello, world")
printfn "    Output: %s\n" sGet

printfn "Calling EchoWithPost via HTTP POST: "
let sPost = channel.EchoWithPost("Hello, world")
printfn "    Output: %s" sPost

Service.close()
