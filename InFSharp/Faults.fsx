#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel

[<ServiceContract>]
type ICalculator =
    [<OperationContract>]
    abstract Divide : double * double -> double

type Calculator() =
    interface ICalculator with
        member this.Divide(n1, n2) =
            match n2 with
            | 0.0  ->
                let ex = new DivideByZeroException()
                raise (new FaultException<DivideByZeroException>(ex, "n2 is 0"))
            | -1.0 -> failwith "Uncontracted exception"
            | _    -> n1 / n2

let host = new InProcHost<Calculator>()
host.AddEndpoint<ICalculator>()
host.Open()

let proxy = host.CreateProxy<ICalculator>()
printfn "%f / %f = %f\n\n" 4.0 2.0 (proxy.Divide(4.0, 2.0))

try
    proxy.Divide(4.0, 0.0) |> ignore
with ex ->
    printfn "%A: %s" (ex.GetType()) ex.Message
    printfn "Proxy state = %A\n\n" (proxy :?> ICommunicationObject).State

// Exception not derived from FaultException will Fault the channel
try
    proxy.Divide(4.0, -1.0) |> ignore
with ex ->
    printfn "%A: %s" (ex.GetType()) ex.Message
    printfn "Proxy state = %A" (proxy :?> ICommunicationObject).State

try
    host.CloseProxy(proxy)
with _ -> ()
host.Close()

