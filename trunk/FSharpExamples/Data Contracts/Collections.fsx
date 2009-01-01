#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
open System.IO
open System.Xml.Linq
open System.Collections
open System.Collections.Generic
open System.Runtime.Serialization


[<CollectionDataContract(Name="MyCollectionOf{0}")>]
type MyCollection<'T>() =
    let items = new ResizeArray<'T>()
    
    member this.Add(item : 'T) =
        items.Add(item)
            
    interface IEnumerable with
        member this.GetEnumerator() =
            items.GetEnumerator() :> IEnumerator 
            
    interface IEnumerable<'T> with
        member this.GetEnumerator() =
            items.GetEnumerator() :> IEnumerator<'T>
        

let c = new MyCollection<string>()
c.Add("item 1")
c.Add("item 2")
c.Add("item 3")

let serializer = new DataContractSerializer(typeof<MyCollection<string>>)
let stream = new MemoryStream()
serializer.WriteObject(stream, c)
stream.Position <- 0L
let reader = new StreamReader(stream)
let doc = XDocument.Parse(reader.ReadToEnd())
printfn "%s" (doc.ToString())

