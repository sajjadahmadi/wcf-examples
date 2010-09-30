#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Threading
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

let main =
    async {
        use host = new ServiceHost(typeof<ServiceSide.Calculator>, new Uri("net.tcp://localhost"))
        host.Open()

        use client = new CalculatorClient(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
        let proxy = client :> ICalculator

        let asyncResult1 = proxy.BeginAdd(2, 3, null, null)
        let asyncResult2 = proxy.BeginAdd(4, 5, null, null)

        let handles = [| asyncResult1.AsyncWaitHandle; asyncResult2.AsyncWaitHandle |]
        WaitHandle.WaitAll(handles) |> ignore

        printfn "2 + 3 = %d" (proxy.EndAdd(asyncResult1))
        printfn "4 + 5 = %d" (proxy.EndAdd(asyncResult2))
        (proxy :?> ICommunicationObject).Close()
        host.Close() }
        
Async.Start main
printfn "Press any key to exit..."
Console.ReadKey(true) |> ignore
