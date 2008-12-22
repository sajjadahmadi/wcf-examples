#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Threading
open System.ServiceModel
open System.Windows.Forms


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


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IFormManager>, binding, "")
host.Open()

let frmAsync = async { Application.Run(new MyForm()) }
Async.Spawn frmAsync

let proxy = ChannelFactory<IFormManager>.CreateChannel(binding, new EndpointAddress("net.tcp://localhost"))
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()
proxy.IncrementLabel()

Thread.Sleep(3000)
(proxy :?> ICommunicationObject).Close()
host.Close()
