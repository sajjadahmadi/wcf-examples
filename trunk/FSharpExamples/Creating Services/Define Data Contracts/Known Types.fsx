#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.Runtime.Serialization

let mutable knownTypes : Type[] = [||]


// When no known types are specified, the example errors
[<DataContract>]
[<KnownType(typeof<Manager>)>]
// You can specify a static method that returns Type[]
//[<KnownType("GetKnownTypes")>]
type Employee() =
    let mutable name = "Employee1"

    [<DataMember>]
    member this.Name
        with get () = name
        and set v = name <- v

    static member GetKnownTypes() =
        // Only doing it this way because of FSI restrictions
        knownTypes
and [<DataContract>] Manager() =
    inherit Employee()

    let mutable title = "The Boss"

    [<DataMember>]
    member this.Title
        with get () = title
        and set v = title <- v


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    // Known types can be specified on the operation
    //[<ServiceKnownType(typeof<Manager>)>]
    abstract MyMethod : Employee -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod(employee) =
            printfn "Received parameter of type %O" (employee.GetType())


knownTypes <- [| typeof<Manager> |]

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

try
    proxy.MyMethod(new Manager())
    (proxy :?> ICommunicationObject).Close()
    host.Close()
with e -> printfn "%s" e.Message
