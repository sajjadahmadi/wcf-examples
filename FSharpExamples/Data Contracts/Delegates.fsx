#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fsx"
open System
open System.Runtime.Serialization

[<DataContract(Name="MyDataContract")>]
type MyDataContract() =
    let mutable f = new Converter<unit,string>(fun () -> "My Result")
    
    [<DataMember>]
    member this.MyDelegate with get() = f
                           and set v = f <- v
      
let x = MyDataContract()
let xData = Serialization.serialize x
printfn "%s" xData
