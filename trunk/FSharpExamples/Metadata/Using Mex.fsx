#light
#r "System.ServiceModel"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.Description.Behaviors.Add(new ServiceMetadataBehavior())
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.AddServiceEndpoint(typeof<IMetadataExchange>, binding, "mex")
host.Open()

let mexClient = new MetadataExchangeClient(binding)

let metadata = mexClient.GetMetadata(new Uri(uri, "mex"), MetadataExchangeClientMode.MetadataExchange)

let importer = new WsdlImporter(metadata)

let endpoints = importer.ImportAllEndpoints()

printfn "%d Endpoint(s) Found\n--------------" endpoints.Count
for endpoint in endpoints do
    printfn "  %s (%s): %A" endpoint.Name (endpoint.Binding.GetType().Name) endpoint.Address
