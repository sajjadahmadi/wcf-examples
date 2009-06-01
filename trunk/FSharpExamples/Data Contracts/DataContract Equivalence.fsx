#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fs"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person", Namespace="")>]
type Person =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember>]
      mutable Age : int }


[<DataContract(Name="Person", Namespace="")>]
type Employee =
    { [<DataMember(Name="Name")>]
      mutable FullName : string;
      
      [<DataMember(Name="Age")>]
      mutable AgeInYears : int
      
      [<DataMember>]
      mutable ServiceYears : int }


let person = { Name = "Ray"; Age = 35 }
let persondata = Serialization.serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = Serialization.deserialize<Employee> persondata
printfn "%A" emp
