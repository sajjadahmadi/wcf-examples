#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System
open System.Threading
open System.Windows.Forms
open System.ServiceModel
open Mcts_70_503


[<ServiceContract>]
type IFormManager =
    [<OperationContract>]
    abstract IncrementLabel : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MyForm() as this =
    inherit Form()
    
    let context = SynchronizationContext.Current
    let mutable host : InProcHost<MyForm> option = None
    let label = new Label(Text="0")
    do this.Controls.Add(label)
    do this.Initialize()
    
    member private this.Initialize() =
        let h = new InProcHost<MyForm>(this)
        h.AddEndpoint<IFormManager>()
        this.FormClosed.Add(fun e -> h.Close())
        h.Open()
        host <- Some h
    
    member this.Host = Option.get host
    
    member this.IncrementLabel() =
        label.Text <- string (int label.Text + 1)
        
    interface IFormManager with
        member this.IncrementLabel() = 
            context.Send((fun _ -> this.IncrementLabel()), null)


let form = new MyForm()
let proxy = form.Host.CreateProxy<IFormManager>()
proxy.IncrementLabel()
form.ShowDialog()
