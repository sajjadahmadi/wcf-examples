#light
#r "System.ServiceModel"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

do Console.WriteLine("Enter the service address:")
let address = Console.ReadLine()
let uri = new Uri(address) // Must be URI to MEX endpoint

let bindingEl = new HttpTransportBindingElement(MaxReceivedMessageSize=327680L) :> BindingElement

let binding = new CustomBinding([| bindingEl |]) :> Binding

let mexClient = new MetadataExchangeClient(binding)

let metadata = mexClient.GetMetadata(uri, MetadataExchangeClientMode.MetadataExchange)

let importer = new WsdlImporter(metadata)

let endpoints = importer.ImportAllEndpoints()

printfn "%d Endpoint(s) Found\n--------------" endpoints.Count
for endpoint in endpoints do
    printfn "  %s (%s): %A" endpoint.Name (endpoint.Binding.GetType().Name) endpoint.Address
