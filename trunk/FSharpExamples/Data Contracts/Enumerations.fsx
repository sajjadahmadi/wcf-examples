#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fs"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="ContactType")>]
type ContactType =
    | [<EnumMember(Value="MyCustomer")>] Customer = 0
    | [<EnumMember>] Vendor   = 1
    | Partner  = 2
      
      
printfn "No Changes To Enum\n-------------------"
printfn "%s\n" (Serialization.serialize ContactType.Vendor)

printfn "Changed Value\n-------------------"
printfn "%s\n" (Serialization.serialize ContactType.Customer)

printfn "Not Included\n-------------------"
try
    Serialization.serialize ContactType.Partner |> ignore
with ex ->
    printfn "%s" ex.Message
