open System
open System.Diagnostics
open System.Collections.Generic
open System.ComponentModel
open System.Threading
open System.Security.Permissions


[<Serializable>]
type WorkItem(m : SendOrPostCallback, state : obj) =
    let handle = new ManualResetEvent(false)
    
    member this.WaitHandle = handle

    member this.Callback() =
        m.Invoke(state)
        handle.Set() |> ignore


type WorkerThread(name : string, context : SynchronizationContext) as this =
    let itemAdded = new AutoResetEvent(false)
    let run (x : WorkerThread) =
        Debug.Assert(SynchronizationContext.Current = null)
        SynchronizationContext.SetSynchronizationContext(context)
        
        while not x.EndLoop do
            while not x.QueueEmpty do
                if x.EndLoop
                    then ()
                    else
                        let workItem : WorkItem = x.GetNext()
                        workItem.Callback()
            itemAdded.WaitOne() |> ignore
            
    let mutable endLoop = false
    let t = new Thread(new ParameterizedThreadStart(fun x -> run (x :?> WorkerThread)), IsBackground = true, Name = name)
    
    let workItemQueue = new Queue<WorkItem>()
    do t.Start(this)

    member this.ThreadId = t.ManagedThreadId

    member this.QueueCount = workItemQueue.Count

    member this.QueueWorkItem(workItem) =
        lock workItemQueue (fun () ->
            workItemQueue.Enqueue(workItem)
            itemAdded.Set())
    
    member this.EndLoop
        with get() = lock this (fun () -> endLoop)
        and set v = lock this (fun () -> endLoop <- v)

    member this.Start() =
        Debug.Assert(t <> null)
        Debug.Assert(t.IsAlive = false)
        t.Start()
      
    member this.QueueEmpty =
        lock workItemQueue (fun () -> workItemQueue.Count <= 0)

    member this.GetNext() =
        if this.QueueEmpty
            then failwith "empty queue"
            else workItemQueue.Dequeue()
            
    member this.Run() =
        run this
    
    member this.Kill() =
        Debug.Assert(t <> null)
        if not t.IsAlive then ()
        else
            this.EndLoop <- true
            itemAdded.Set() |> ignore
            t.Join()
            if not (itemAdded = null) then itemAdded.Close()
                

[<SecurityPermission(SecurityAction.Demand, ControlThread = true)>]
type AffinitySynchronizer(name : string) as this =
    inherit SynchronizationContext()
    
    let mutable t : WorkerThread option = None
    
    member private this.WorkerThread =
        match t with
        | Some t' -> t'
        | None    ->
            let t' = new WorkerThread(name, this)
            t <- Some t'
            t'

    member this.QueueCount = this.WorkerThread.QueueCount

    override this.CreateCopy() = this :> SynchronizationContext
    
    override this.Post(m : SendOrPostCallback, state) =
        let workItem = new WorkItem(m, state)
        this.WorkerThread.QueueWorkItem(workItem) |> ignore

    override this.Send(m : SendOrPostCallback, state) =
        if SynchronizationContext.Current = (this :> SynchronizationContext)
            then m.Invoke(state)
            else
                let workItem = new WorkItem(m, state)
                this.WorkerThread.QueueWorkItem(workItem) |> ignore
                workItem.WaitHandle.WaitOne() |> ignore
    
    interface IDisposable with
        member this.Dispose() =
            this.WorkerThread.Kill()

let s = new AffinitySynchronizer("Example")
s.Post((fun _ -> Thread.Sleep(1000); printfn "%d" Thread.CurrentThread.ManagedThreadId), null)
printfn "%d threads in queue" s.QueueCount
s.Send((fun _ -> printfn "%d" Thread.CurrentThread.ManagedThreadId), null)
printfn "%d threads in queue" s.QueueCount
