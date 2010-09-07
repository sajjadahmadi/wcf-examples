// Adapted from http://msdn.microsoft.com/en-us/library/bb412178.aspx
#r "System.ServiceModel"
#r "System.ServiceModel.Web"
#load "_Service.fsx"

printfn "Press enter to quit..."
System.Console.ReadLine() |> ignore

Service.close()
