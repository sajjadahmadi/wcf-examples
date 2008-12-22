#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels


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


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<Calculator>, [| uri |])
host.AddServiceEndpoint(typeof<ICalculator>, binding, "")
host.Open()

let proxy = ChannelFactory<ICalculator>.CreateChannel(binding, new EndpointAddress("net.tcp://localhost"))
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
    (proxy :?> ICommunicationObject).Close()
with _ -> ()
host.Close()

