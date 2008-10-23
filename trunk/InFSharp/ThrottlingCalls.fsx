#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Description

type DateTime with
    static member CurrentTime =
        let now = DateTime.Now
        sprintf "%d:%d:%d" now.Hour now.Minute now.Second

[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit

type MyService() =
    
    interface IMyContract with
        member this.MyMethod() =
            Threading.Thread.Sleep(6000)
            printfn "%s: %s" "MyService.MyMethod()" DateTime.CurrentTime

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>(new NetTcpBinding())
host.AddEndPoint<IMyContract>(new NetNamedPipeBinding(SendTimeout = new TimeSpan(0, 0, 5)))
let throttle = new ServiceThrottlingBehavior(MaxConcurrentCalls = 1)
host.InnerHost.Description.Behaviors.Add(throttle)
host.Open()

let proxy1 = host.CreateProxy<IMyContract, NetTcpBinding>()
let call1 = async{ proxy1.MyMethod() }
Async.Run call1

let proxy2 = host.CreateProxy<IMyContract, NetNamedPipeBinding>()
let call2 = async{ proxy2.MyMethod() }
try
    Async.Run call2
    host.CloseProxy(proxy2)
with ex -> printfn "\n%s\n" ex.Message

host.CloseProxy(proxy1)
host.Close()
