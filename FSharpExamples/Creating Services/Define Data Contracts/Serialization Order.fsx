#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
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
let persondata = serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = deserialize<Employee> persondata
printfn "%A" emp