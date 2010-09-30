#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Threading
open System.ServiceModel
open System.ServiceModel.Channels
open System.Windows.Forms


type SafeLabel() =
    inherit Label()
    
    let context = SynchronizationContext.Current
    
    let getTxt (x : SafeLabel) =
        let v = ref ""
        context.Send((fun _ -> v := x.BaseText), null)
        !v
        
    let setTxt (x : SafeLabel) v =
        context.Send((fun _ -> x.BaseText <- v), null)
        
    override this.Text with get() = getTxt this
                       and set v = setTxt this v

    member private this.BaseText with get() = base.Text
                                 and set v = base.Text <- v

type MyForm() as this =
    inherit Form()
    
    let counter = new SafeLabel(Text = "0")
    let inc() =
        let i = int counter.Text
        counter.Text <- string (i+1)

    do this.Controls.Add(counter)

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
            form.Increment()


let host = new ServiceHost(typeof<MyService>, new Uri("net.tcp://localhost"))
host.Open()

let frmAsync = async { Application.Run(new MyForm()) }
Async.Start frmAsync

let proxy = ChannelFactory<IFormManager>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()

printfn "Press any key to exit..."
Console.ReadKey(true) |> ignore
(proxy :?> ICommunicationObject).Close()
host.Close()
