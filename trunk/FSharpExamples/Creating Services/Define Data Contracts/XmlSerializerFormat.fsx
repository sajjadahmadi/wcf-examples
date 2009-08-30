#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher
Console.Clear()


[<ServiceContract>]
[<XmlSerializerFormat(Style = OperationFormatStyle.Rpc, Use = OperationFormatUse.Encoded)>]
type ISomeLegacyService =
    [<OperationContract>]
    abstract SomeOp1 : string -> unit


[<PrintMessagesToConsole>]
type MyService() =
    interface ISomeLegacyService with
        member this.SomeOp1(name) =
            printfn "Hi, %s!" name
            

example<MyService, ISomeLegacyService> (fun proxy ->
    proxy.SomeOp1("You") |> ignore)
