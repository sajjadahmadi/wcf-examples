#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fs"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person")>]
type Person =
    { [<DataMember(Order=1)>]
      mutable Name : string;
      
      [<DataMember(Order=2)>]
      mutable Age : int }


[<DataContract(Name="Person")>]
type Employee =
    { [<DataMember(Order=1)>]
      mutable Name : string;
      
      [<DataMember(Order=2)>]
      mutable Age : int }


let person = { Name = "Ray"; Age = 35 } : Person
let persondata = Serialization.serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = Serialization.deserialize<Employee> persondata
printfn "%A" emp