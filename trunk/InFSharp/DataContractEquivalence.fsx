#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "../../ref/Helpers.fsx"
open System
open System.Runtime.Serialization

[<DataContract(Name="Person")>]
type Person =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember>]
      mutable Age : int }

[<DataContract(Name="Person")>]
type Employee =
    { [<DataMember(Name="Name")>]
      mutable FullName : string;
      
      [<DataMember(Name="Age")>]
      mutable AgeInYears : int
      
      [<DataMember>]
      mutable ServiceYears : int }

let person = { Name = "Ray"; Age = 35 }
let persondata = Helpers.serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = Helpers.deserialize<Employee> persondata
printfn "%A" emp