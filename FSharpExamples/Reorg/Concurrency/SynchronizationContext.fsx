open System.Threading

let runSync() =
    let context = SynchronizationContext.Current
    let doWork() =
        context.Post((fun state ->
            printfn "Worker Thread: %d" Thread.CurrentThread.ManagedThreadId), null)
    let t = new Thread(doWork)
    t.Start()


SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())
printfn "Main thread on thread #%d" Thread.CurrentThread.ManagedThreadId
let st = runSync()
printfn "Press any key to exit..."
System.Console.ReadKey(true) |> ignore
