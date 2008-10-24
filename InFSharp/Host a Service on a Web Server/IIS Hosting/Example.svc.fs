#light
namespace IISHosting
open System.ServiceModel

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My F# Result"
