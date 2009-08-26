#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher
Console.Clear()


[<ServiceContract>]
type IContract =
    [<OperationContract>]
    [<DataContractFormat(Style = OperationFormatStyle.Rpc)>]
    abstract RpcOperation : string -> string

    [<OperationContract>]
    [<DataContractFormat(Style = OperationFormatStyle.Document)>]
    abstract DocumentOperation : string -> string


[<PrintMessagesToConsole>]
type Service() =
    interface IContract with
        member this.RpcOperation(name) =
            sprintf "Hi, %s!" name
        
        member this.DocumentOperation(name) =
            sprintf "Hi, %s!" name


let host = new ExampleHost<Service, IContract>()
host.Open()

let proxy = host.CreateProxy()
proxy.RpcOperation("You")
proxy.DocumentOperation("You")
