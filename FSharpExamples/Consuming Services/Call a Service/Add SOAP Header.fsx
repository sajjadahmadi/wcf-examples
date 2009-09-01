#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
open System.ServiceModel
Console.Clear()


[<DataContract>]
type MyCustomType =
    { [<DataMember>] mutable Member1 : string }
    

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            let hdrs = OperationContext.Current.IncomingMessageHeaders
            printfn "\nIncomingHeaders\n-----------------"
            for h in hdrs do
                printfn "%s: %A\n" h.Name h


example<MyService, IMyContract>(fun _ proxy ->
    let headerData = { Member1 = "value" }
    let messageHeader = new MessageHeader<MyCustomType>(headerData)

    let scope = new OperationContextScope(proxy :?> IContextChannel)
    let header = messageHeader.GetUntypedHeader("MyCustomType", "ServiceModelEx")
    OperationContext.Current.OutgoingMessageHeaders.Add(header)
    proxy.MyMethod())
