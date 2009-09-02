#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : string * int -> string


type MyService() =
    interface IMyContract with
        member this.MyMethod(s, n) = "result"


type PrintToConsoleParameterInspector() =
    interface IParameterInspector with
        member this.AfterCall(operationName, outputs, returnValue, correlationState) =
            printfn "AFTER CALL"
            printfn "  Operation: %s" operationName
            for output in outputs do
                printfn "    param: %A" output
            printfn "    result: %A" returnValue
            printfn "    correlationState: %A" correlationState
        
        member this.BeforeCall(operationName, inputs) =
            printfn "BEFORE CALL"
            printfn "  Operation: %s" operationName
            for input in inputs do
                printfn "    input: %A" input
            box "state"


example2<MyService, IMyContract>
    (fun() ->
        let host = new ExampleHost<MyService, IMyContract>()
        let inspector = new PrintToConsoleParameterInspector()
        let behavior = new ApplyParameterInspectorBehavior(inspector)
        host.Description.Endpoints.[0].Contract.Operations.[0].Behaviors.Add(behavior)
        host)
    (fun _ proxy ->
        proxy.MyMethod("input", 1) |> ignore)


