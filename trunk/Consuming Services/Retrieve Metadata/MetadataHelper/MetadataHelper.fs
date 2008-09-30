#light
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.Text.RegularExpressions

module MetadataHelper =

    let internal messageMultiplier = 5
    
    let internal setMessageSize (el: TransportBindingElement) =
        el.MaxReceivedMessageSize <- el.MaxReceivedMessageSize * (int64 messageMultiplier)
        
    let internal (|NetTcp|NetPipe|Http|Https|) = function
        | "net.tcp"  -> NetTcp (new TcpTransportBindingElement())
        | "net.pipe" -> NetPipe (new NamedPipeTransportBindingElement())
        | "http"     -> Http (new HttpTransportBindingElement())
        | "https"    -> Https (new HttpsTransportBindingElement())
        | _          -> failwith "invalid scheme"

    let internal httpGetAddr (addr: string) =
        if Regex.IsMatch("?wsdl$", addr, RegexOptions.IgnoreCase)
            then addr
            else addr + "?wsdl"

    let internal serviceName<'C> =
        let t = typeof<'C>
        let attributes = t.GetCustomAttributes(typeof<ServiceContractAttribute>, false)
        
        if not t.IsInterface then None
        else if attributes.Length = 0 then None
        else
            let attr = attributes.[0] :?> ServiceContractAttribute
            match attr.Name,attr.Namespace with
            | null,null -> Some (t.ToString(), "http://tempuri.org")
            | null,ns   -> Some (t.ToString(), ns)
            | n,null    -> Some (n, "http://tempuri.org")
            | n,ns      -> Some (n, ns)
            
    let queryHttpEndpoint (mexAddress: string) (el: BindingElement) =
        let addr = httpGetAddr mexAddress
        let binding = new CustomBinding([| el |])
        let mexClient = new MetadataExchangeClient(binding)
        let metadata = mexClient.GetMetadata(new Uri(addr), MetadataExchangeClientMode.HttpGet)
        let importer = new WsdlImporter(metadata)
        
        importer.ImportAllEndpoints()

    let tryQueryHttpEndpoint (mexAddress: string) (el: BindingElement) =
        try
            Some (queryHttpEndpoint mexAddress el)
        with _ -> None
    
    let queryMexEndpoint (mexAddress: string) (bindingElement: BindingElement) =
        let binding = new CustomBinding([| bindingElement |])
        let mexClient = new MetadataExchangeClient(binding)
        let metadata = mexClient.GetMetadata(new EndpointAddress(mexAddress))
        let importer = new WsdlImporter(metadata)
        
        importer.ImportAllEndpoints()
    
    let tryQueryMexEndpoint (mexAddress: string) (bindingElement: BindingElement) =
        try
            Some (queryMexEndpoint mexAddress bindingElement)
        with _ -> None

    let getEndpoints (mexAddress: string) =
        let address = new Uri(mexAddress)
        
        match address.Scheme with
        | NetTcp el  ->
            setMessageSize el
            tryQueryMexEndpoint mexAddress el
        | NetPipe el ->
            setMessageSize el
            tryQueryMexEndpoint mexAddress el
        | Http el    ->
            setMessageSize el
            match tryQueryMexEndpoint mexAddress el with
            | None -> tryQueryHttpEndpoint mexAddress el
            | es   -> es
        | Https el   ->
            setMessageSize el
            match tryQueryMexEndpoint mexAddress el with
            | None -> tryQueryHttpEndpoint mexAddress el
            | es   -> es
    
    let queryContract (mexAddress: string) (contractNamespace: string) (contractName: string) =
       
        let matches (endpoint: ServiceEndpoint) =
            endpoint.Contract.Namespace = contractNamespace &&
            endpoint.Contract.Name = contractName
    
        if String.IsNullOrEmpty(contractNamespace) then false
        else if String.IsNullOrEmpty(contractName) then false
        else
            match getEndpoints mexAddress with
            | None    -> false
            | Some es ->
                es
                |> Seq.exists matches

    let queryContractType<'C> (mexAddress: string) =
        match serviceName<'C> with
        | None        -> false
        | Some (n,ns) -> queryContract mexAddress n ns
