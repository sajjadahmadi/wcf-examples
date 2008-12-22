#light
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
        let uri = new Uri("net.tcp://localhost")
        let binding = new NetTcpBinding()
        use host = new ServiceHost(typeof<ServiceSide.Calculator>, [| uri |])
        host.AddServiceEndpoint(typeof<ServiceSide.ICalculator>, binding, "") |> ignore
        host.Open()

        let address = new EndpointAddress("net.tcp://localhost")
        use client = new CalculatorClient(binding, address)
        let proxy = client :> ICalculator

        let asyncResult1 = proxy.BeginAdd(2, 3, null, null)
        let asyncResult2 = proxy.BeginAdd(4, 5, null, null)

        let handles = [| asyncResult1.AsyncWaitHandle; asyncResult2.AsyncWaitHandle |]
        WaitHandle.WaitAll(handles) |> ignore

        printfn "2 + 3 = %d" (proxy.EndAdd(asyncResult1))
        printfn "4 + 5 = %d" (proxy.EndAdd(asyncResult2))
        client.Close()
        host.Close() }
        
Async.Spawn main
printfn "Press any key to exit..."
Console.ReadKey(true) |> ignore
