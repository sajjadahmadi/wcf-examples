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
    abstract Add : double * double -> double
    
    [<OperationContract>]
    [<FaultContract(typeof<DivideByZeroException>)>]
    abstract Divide : double * double -> double

type Calculator() =
    interface ICalculator with
        member this.Add(n1, n2) =
            n1 + n2
            
        member this.Divide(n1, n2) =
            if n2 = 0.0 then
                let ex = new DivideByZeroException()
                raise (new FaultException<DivideByZeroException>(ex, "n2 is 0"))
            n1 / n2

let host = new InProcHost<Calculator>()
host.AddEndpoint<ICalculator>()
host.Open()

let proxy = host.CreateProxy<ICalculator>()
printfn "%f / %f = %f" 4.0 2.0 (proxy.Divide(4.0, 2.0))

try
    proxy.Divide(4.0, 0.0) |> ignore
with :? FaultException<DivideByZeroException> as ex -> 
    printfn "%A: %s" (ex.GetType()) ex.Message

// When the service throws an exception listed in the service-side fault
//  contract, the exception will not fault the communication channel.
printfn "Proxy state = %A" (proxy :?> ICommunicationObject).State

host.CloseProxy(proxy)
host.Close()

