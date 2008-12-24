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


let uri = new Uri("http://localhost")
let binding = new BasicHttpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.Description.Behaviors.Add(new ServiceMetadataBehavior(HttpGetEnabled = true))
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let mexClient = new MetadataExchangeClient(binding)

let metadata = mexClient.GetMetadata(new Uri(uri, "?wsdl"), MetadataExchangeClientMode.HttpGet)

let importer = new WsdlImporter(metadata)

let endpoints = importer.ImportAllEndpoints()

printfn "%d Endpoint(s)\n--------------" endpoints.Count
for endpoint in endpoints do
    printfn "  %s (%s): %A" endpoint.Name (endpoint.Binding.GetType().Name) endpoint.Address
