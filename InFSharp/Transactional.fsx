#light
#r "System.Transactions"
open System.Transactions

type ITransactional<'T> =
    inherit ISinglePhaseNotification
    abstract Value : 'T with get, set
    abstract Rollback : unit -> unit
    

let enlist<'T> (x : ITransactional<'T>) =
    if Transaction.Current <> null then
        Transaction.Current.EnlistVolatile((x :> ISinglePhaseNotification), EnlistmentOptions.None) 
        |> ignore
    x
    
let transactional<'T> (v : 'T) =
    let valStore = ref v
    enlist { new ITransactional<'T> with
        member this.Value with get() = !valStore
                          and set newVal = valStore := newVal
                          
        member this.Rollback() =
            valStore := v
            
        member this.SinglePhaseCommit(singlePhaseEnlistment) =
            printfn "SINGLEPHASECOMMIT"
            singlePhaseEnlistment.Committed()

        member this.Commit(enlistment) =
            printfn "COMMIT"
            enlistment.Done()
        
        member this.InDoubt(enlistment) =
            printfn "INDOUBT"
            enlistment.Done()
        
        member this.Prepare(preparingEnlistment) =
            printfn "PREPARE"
            preparingEnlistment.Prepared()
        
        member this.Rollback(enlistment) =
            printfn "ROLLBACK"
            this.Rollback()
            enlistment.Done() }
    


let scope1 = new TransactionScope()
let ts1 = transactional(1)
printfn "%A" ts1.Value
ts1.Value <- 2
printfn "%A" ts1.Value
scope1.Dispose()
printfn "%A\n\n" ts1.Value  

let scope2 = new TransactionScope()
let ts2 = transactional(1)
printfn "%A" ts2.Value
ts2.Value <- 2
printfn "%A" ts2.Value
scope2.Complete()
scope2.Dispose()
printfn "%A" ts2.Value
