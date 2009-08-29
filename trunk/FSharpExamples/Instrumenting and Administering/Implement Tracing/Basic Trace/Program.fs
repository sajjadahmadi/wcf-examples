open System.ServiceModel

[<ServiceContract>]
type IContract =
    [<OperationContract>]
    abstract Operation : unit -> unit
    
type Service() =
    interface IContract with
        member this.Operation() =
            printfn "Service Operation Called"


let host = new ExampleHost<Service, IContract>()
host.Open()

let proxy = host.CreateProxy()
proxy.Operation()

host.Close()
printfn "Done!"

