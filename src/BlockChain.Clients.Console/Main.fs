open System
open BlockChain

[<EntryPoint>]
let main argv =
    let startTime = DateTime.Now
    let blockChain = Blockchain.empty |> Blockchain.addGenesisBlock

    let transaction =
        { Transaction.empty with SenderAddress = "FnStack"
                                 RecipientAddress = "Rajivhost"
                                 Amount = 10000m }

    let blockChain = (blockChain, transaction) |> Blockchain.addTransaction
    let blockChain =
        (blockChain, "miner1") |> Blockchain.processPendingTransactions
    printf "%A" blockChain
    0 // return an integer exit code
