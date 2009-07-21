#r "System.ServiceModel"
#r "System.ServiceModel.Web"
open System.ServiceModel
open System.ServiceModel.Description

#load "Service.fs"
Service.start()

let factory = new ChannelFactory<Service.IService>(new WebHttpBinding(), "http://localhost:8888")
factory.Endpoint.Behaviors.Add(new WebHttpBehavior())

let channel = factory.CreateChannel()

printfn "Calling EchoWithGet via HTTP GET: "
let sGet = channel.EchoWithGet("Hello, world")
printfn "    Output: %s" sGet

printfn "\nThis can also be accomplished be navigating to <http://localhost:8000/EchoWithGet?s=Hello, world> in a web browser"

printfn "Calling EchoWithPost via HTTP POST: "
let sPost = channel.EchoWithPost("Hello, world")
printfn "    Output: %s" sPost

Service.close()
