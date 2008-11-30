#light
#r "System.Transactions"
open System.Transactions

type Transactional<'T>(value : 'T) as this =
    let mutable currentValue = value
    do Transaction.Current.EnlistVolatile((this :> ISinglePhaseNotification), EnlistmentOptions.None)
    |> ignore
    
    member this.Value with get() = currentValue
                      and set v  = currentValue <- v
    
    member this.Rollback() =
        currentValue <- value
    
    interface ISinglePhaseNotification with
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
            enlistment.Done()


let scope1 = new TransactionScope()
let ts1 = new Transactional<int>(1)
printfn "%A" ts1.Value
ts1.Value <- 2
printfn "%A" ts1.Value
scope1.Dispose()
printfn "%A\n\n" ts1.Value

let scope2 = new TransactionScope()
let ts2 = new Transactional<int>(1)
printfn "%A" ts2.Value
ts2.Value <- 2
printfn "%A" ts2.Value
scope2.Complete()
scope2.Dispose()
printfn "%A" ts2.Value
