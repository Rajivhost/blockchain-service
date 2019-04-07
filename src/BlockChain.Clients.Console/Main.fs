open System
open BlockChain

[<EntryPoint>]
let main argv =
    let startTime = DateTime.Now

    let blockChain = Blockchain.empty |> Blockchain.addGenesisBlock

    printf "%A" blockChain



    0 // return an integer exit code
