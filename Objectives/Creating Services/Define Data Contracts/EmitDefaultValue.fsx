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
      
      [<DataMember(EmitDefaultValue=false)>]
      mutable Age : int }

let p1 = { Name = "Ray Vernagus";
           Age   = 0 }
printfn "%s\n\n----------------------\n" (Helpers.serialize p1)

let p2 = { Name = "Ray Vernagus";
           Age   = 1 }
printfn "%s" (Helpers.serialize p2)
