open System
open System.Threading
open System.ComponentModel

let runSync() =
    let operation = AsyncOperationManager.CreateOperation(null)
    let doWork() =
        operation.Post((fun state ->
            printfn "Worker Thread on thread #%d" Thread.CurrentThread.ManagedThreadId), null)
    let t = new Thread(doWork)
    t.Start()


// when using the AsyncOperation class there is no need to set the SynchronizationContext
printfn "Main thread on thread #%d" Thread.CurrentThread.ManagedThreadId
let st = runSync()
printfn "Press any key to exit..."
Console.ReadKey(true) |> ignore
