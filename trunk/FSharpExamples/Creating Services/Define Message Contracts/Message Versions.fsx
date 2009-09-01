#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System.ServiceModel.Channels
open System.Runtime.Serialization
System.Console.Clear()


[<DataContract(Name = "Employee")>]
type Employee =
    { [<DataMember>] mutable FirstName : string;
      [<DataMember>] mutable LastName : string;
      [<DataMember>] mutable Department : string }


let emp = { FirstName = "Ray"; LastName = "Vernagus"; Department = "Developer Division" }

// SOAP 1.1
let message1 = Message.CreateMessage(MessageVersion.Soap11, "http://TestNamespace", emp)
printfn "SOAP 1.1\n---------------------------------\n%A\n" message1

// SOAP 1.2
let message2 = Message.CreateMessage(MessageVersion.Soap12, "http://TestNamespace", emp)
printfn "SOAP 1.2\n---------------------------------\n%A\n" message2

// None
let message3 = Message.CreateMessage(MessageVersion.None, "http://TestNamespace", emp)
printfn "None\n---------------------------------\n%A\n" message3
