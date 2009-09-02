#r "System.Runtime.Serialization"
#r "System.ServiceModel"
#r @"..\..\bin\Mcts70_503.dll"
open System.Net.Security
open System.ServiceModel.Channels
open System.ServiceModel
open System.ServiceModel.Channels
System.Console.Clear()


[<MessageContract>]
type Person(age, firstName, lastName) =
    let mutable age = age
    let mutable firstName = firstName
    let mutable lastName = lastName
    
    new() = Person(-1, @"N\A", @"N\A")
    
//    [<MessageBodyMember(ProtectionLevel = ProtectionLevel.EncryptAndSign)>]
//    member this.Age with get() = age
//                     and set v = age <- v
    
    [<MessageBodyMember(ProtectionLevel = ProtectionLevel.Sign)>]
    member this.FirstName with get() = firstName
                           and set v = firstName <- v
      
    [<MessageBodyMember(ProtectionLevel = ProtectionLevel.None)>] 
    member this.LastName with get() = lastName
                          and set v = lastName <- v
      

[<ServiceContract>]
type IContract =
    [<OperationContract>]
    abstract AnOperation : Person -> Person


type Service() =
    interface IContract with
        member this.AnOperation(person) = person


// Notes:
//   Requires Fiddler proxy to be running
//   Message body is only visible when ProtectionLevel < EncryptAndSign
//   If == EncryptAndSign, then entire body is encrypted?

example2<Service, IContract>
    (fun() ->
        let binding = WSHttpBinding()
        binding.Security.Mode <- SecurityMode.Message
        let host = new ExampleHost<Service, IContract>(binding, "http://localhost:8000")
        host.IncludeExceptionDetails()
        host)
    (fun host _ ->
        let binding = host.Description.Endpoints.[0].Binding
        
        let factory = new ChannelFactory<IContract>(binding, "http://localhost:8888")
        let proxy = factory.CreateChannel()
        let person = Person(36, "Joe", "Schmo")
        proxy.AnOperation(person) |> ignore)
