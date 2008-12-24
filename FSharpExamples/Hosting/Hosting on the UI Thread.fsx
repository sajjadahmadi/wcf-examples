#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Windows.Forms
open System.ServiceModel


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyMethod() = "MyService.MyMethod()"


type HostForm() as this =
    inherit Form()
    
    let uri = new Uri("net.tcp://localhost")
    let binding = new NetTcpBinding()
    let host = new ServiceHost(typeof<MyService>, [| uri |])
    let btn = new Button(Text = "Service Call")
    
    do btn.Click.Add(fun _ -> this.ServiceCall())
    do this.Controls.Add(btn)
    do host.AddServiceEndpoint(typeof<IMyContract>, binding, "") |> ignore
    do this.FormClosed.Add(fun _ -> host.Close())
    do host.Open()

    member this.ServiceCall() =
        let call = async {
            let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
            MessageBox.Show(proxy.MyMethod()) |> ignore
            (proxy :?> ICommunicationObject).Close() }
        Async.Spawn call


Application.Run(new HostForm())
