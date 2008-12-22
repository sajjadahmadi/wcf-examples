#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fsx"
open System
open System.Runtime.Serialization

[<DataContract(Name="ContactType")>]
type ContactType =
    | [<EnumMember(Value="MyCustomer")>] Customer = 0
    | [<EnumMember>] Vendor   = 1
    | Partner  = 2
      
let cType = ContactType.Customer
let data = Serialization.serialize cType
printfn "%s" data
