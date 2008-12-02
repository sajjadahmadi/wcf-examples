#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System.Threading
open System.ServiceModel
open System.Windows.Forms
open Mcts_70_503


type MyForm() as this =
    inherit Form()
    
    let context = SynchronizationContext.Current
    let counter = new Label(Text = "0")
    let inc() =
        let i = int counter.Text
        counter.Text <- string (i+1)

    do this.Controls.Add(counter)

    member this.SynchronizationContext = context
    
    member this.Increment() = inc()
    

[<ServiceContract>]
type IFormManager =
    [<OperationContract>]
    abstract IncrementLabel : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)>]
type MyService() =
    interface IFormManager with
        member this.IncrementLabel() =
            let form = Application.OpenForms.[0] :?> MyForm
            form.SynchronizationContext.Send((fun state ->
                form.Increment()), null)


let host = new InProcHost<MyService>()
host.AddEndpoint<IFormManager>()
host.Open()

let frmAsync = async { Application.Run(new MyForm()) }
Async.Spawn frmAsync

let proxy = host.CreateProxy<IFormManager>()
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()

Thread.Sleep(3000)
host.CloseProxy(proxy)
host.Close()
