#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Description

[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit

type MyService() =
    interface IMyContract with
        member this.MyMethod() = printfn "%s" "MyService.MyMethod()"

let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>(new NetNamedPipeBinding(SendTimeout = new TimeSpan(0, 0, 5)))
let throttle = new ServiceThrottlingBehavior(MaxConcurrentInstances = 1)
host.InnerHost.Description.Behaviors.Add(throttle)
host.Open()

let proxy1 = host.CreateProxy<IMyContract>()
proxy1.MyMethod()

let proxy2 = host.CreateProxy<IMyContract>()
try
    proxy2.MyMethod()
    host.CloseProxy(proxy2)
with ex -> printfn "\n%s\n" ex.Message

host.CloseProxy(proxy1)
host.Close()
