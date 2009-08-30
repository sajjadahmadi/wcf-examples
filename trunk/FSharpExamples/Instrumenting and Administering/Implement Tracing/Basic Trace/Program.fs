open System.ServiceModel

[<ServiceContract>]
type IContract =
    [<OperationContract>]
    abstract Operation : unit -> unit
    
type Service() =
    interface IContract with
        member this.Operation() =
            printfn "Service Operation Called"


example<Service, IContract> (fun proxy ->
    proxy.Operation())

printfn "Done!"

