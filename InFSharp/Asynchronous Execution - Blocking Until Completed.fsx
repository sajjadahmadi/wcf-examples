#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open Mcts_70_503

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


let host = new InProcHost<ServiceSide.Calculator>()
host.AddEndpoint<ServiceSide.ICalculator>()
host.Open()

let binding = new NetNamedPipeBinding()
let address = new EndpointAddress("net.pipe://localhost")
let client = new CalculatorClient(binding, address) :> ICalculator

let asyncResult = client.BeginAdd(2, 3, null, null)

// some other work

asyncResult.AsyncWaitHandle.WaitOne()
printfn "2 + 3 = %d" (client.EndAdd(asyncResult))
    
printfn "Press any key to exit..."
Console.ReadKey(true)
