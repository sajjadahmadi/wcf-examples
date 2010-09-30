#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels


module ServiceSide =
    // Service Side
    [<ServiceContract>]
    type ICalculator =
        [<OperationContract>]
        abstract Add : int * int -> int
        

    type Calculator() =
        interface ICalculator with
            member this.Add(number1, number2) =
                number1 + number2
    

// Client Side
[<ServiceContract>]
type ICalculator =
    [<OperationContract()>]
    abstract Add : int * int -> int
    
    [<OperationContract(AsyncPattern = true)>]
    abstract BeginAdd : int * int * AsyncCallback * obj -> IAsyncResult

    abstract EndAdd : IAsyncResult -> int


type CalculatorClient(binding : Binding, address : EndpointAddress) =
    inherit ClientBase<ICalculator>(binding, address)
    
    interface ICalculator with
        member this.Add(number1, number2) =
            this.Channel.Add(number1, number2)
            
        member this.BeginAdd(number1, number2, callback, asyncState) =
            this.Channel.BeginAdd(number1, number2, callback, asyncState)

        member this.EndAdd(result) =
            this.Channel.EndAdd(result)


let host = new ServiceHost(typeof<ServiceSide.Calculator>, new Uri("net.tcp://localhost"))
host.Open()

let client = new CalculatorClient(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
let proxy = client :> ICalculator

proxy.BeginAdd(2, 3, (fun result ->
    printfn "2 + 3 = %d" (proxy.EndAdd(result))), null)

printfn "Press any key to exit..."
Console.ReadKey(true) |> ignore
(proxy :?> ICommunicationObject).Close()
host.Close()
