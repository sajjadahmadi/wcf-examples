#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "../../ref/Helpers.fsx"
open System
open System.Runtime.Serialization

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

let person = { new Person with Name = "Ray" and Age = 35 }
let persondata = Helpers.serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = Helpers.deserialize<Employee> persondata
printfn "%A" emp