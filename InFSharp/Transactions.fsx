#light
#r "System.Transactions"
open System.Transactions

let abortedTx() =
    // f1 never completes its transaction, the transaction is aborted
    let f1() =
        use tx = new TransactionScope()
        ()
    
    let f2() =
        use tx = new TransactionScope()
        f1()
        tx.Complete()
    f2()

try
    printfn "Aborted example\n--------------------------"
    abortedTx()
with ex -> printfn "%s\n" ex.Message

let completedTx() =
    let f1() =
        use tx = new TransactionScope()
        tx.Complete()
    
    let f2() =
        use tx = new TransactionScope()
        Transaction.Current.TransactionCompleted.Add(fun e -> printfn "%A" e.Transaction.TransactionInformation.Status)
        f1()
        tx.Complete()
    f2()

printfn "Completed example\n--------------------------"
completedTx()
