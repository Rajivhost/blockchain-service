namespace BlockChain

open System
open System.Text
open System.Security.Cryptography
open Newtonsoft.Json

type Transaction =
    { SenderAddress : string
      RecipientAddress : string
      Amount : decimal
      TimeStamp : DateTimeOffset }

module Transaction =
    let empty =
        { SenderAddress = String.Empty
          RecipientAddress = String.Empty
          Amount = 0m
          TimeStamp = DateTimeOffset.Now }

////////////////////////////////////////////////////////////////////////////////////////////////////
// Assembling the block
////////////////////////////////////////////////////////////////////////////////////////////////////
type Block =
    { Index : int
      TimeStamp : DateTimeOffset
      PreviousHash : string
      Hash : string
      Transactions : Transaction list
      Nonce : int }

module Block =
    let empty =
        { Index = 0
          TimeStamp = DateTimeOffset.Now
          PreviousHash = String.Empty
          Hash = String.Empty
          Transactions = List.Empty
          Nonce = 0 }

    let calculateHash block =
        let sha256 = SHA256.Create()
        let input =
            sprintf "%A-%s-%s-%i" block.TimeStamp block.PreviousHash
                (JsonConvert.SerializeObject(block.Transactions)) block.Nonce
        let inputBytes = input |> Encoding.ASCII.GetBytes
        let outputBytes = sha256.ComputeHash(inputBytes)
        Convert.ToBase64String(outputBytes)

    let mine (block, difficulty) =
        let leadingZeros = new string('0', difficulty)
        let mutable nonce = block.Nonce
        let mutable hash = block.Hash
        while hash |> String.IsNullOrEmpty
              || hash.Substring(0, difficulty) <> leadingZeros do
            nonce <- nonce + 1
            hash <- { block with Nonce = nonce
                                 Hash = hash }
                    |> calculateHash
        { block with Nonce = nonce
                     Hash = hash }

////////////////////////////////////////////////////////////////////////////////////////////////////
// Assembling the block chain
////////////////////////////////////////////////////////////////////////////////////////////////////
type Blockchain =
    { PendingTransactions : Transaction list
      Chain : Block list
      Difficulty : int
      Reward : decimal }

module Blockchain =
    let empty =
        { PendingTransactions = List.Empty
          Chain = List.Empty
          Difficulty = 2
          Reward = 1m }

    let createGenesisBlock blockChain =
        let genesisBlock = (Block.empty, blockChain.Difficulty) |> Block.mine
        genesisBlock

    let addGenesisBlock blockChain =
        let genesisBlock = blockChain |> createGenesisBlock
        let chain = blockChain.Chain @ [ genesisBlock ]
        let blockChain = { blockChain with Chain = chain }
        blockChain

    let getLatestBlock blockChain =
        let latestBlock = blockChain.Chain |> List.last
        latestBlock

    let addTransaction (blockChain, transaction) =
        let pendingTransactions =
            blockChain.PendingTransactions @ [ transaction ]
        let blockChain =
            { blockChain with PendingTransactions = pendingTransactions }
        blockChain

    let addBlock (blockChain, block) =
        let latestBlock = blockChain |> getLatestBlock

        let block =
            { block with Index = latestBlock.Index + 1
                         PreviousHash = latestBlock.Hash }

        let block = (block, blockChain.Difficulty) |> Block.mine
        let chain = blockChain.Chain @ [ block ]
        let blockChain = { blockChain with Chain = chain }
        blockChain

    let processPendingTransactions (blockChain, minerAddress) =
        let latestBlock = blockChain |> getLatestBlock

        let block =
            { Block.empty with PreviousHash = latestBlock.Hash
                               Transactions = blockChain.PendingTransactions }

        let blockChain = (blockChain, block) |> addBlock
        let blockChain = { blockChain with PendingTransactions = List.empty }

        // COINBASE
        let transaction =
            { Transaction.empty with RecipientAddress = minerAddress
                                     Amount = blockChain.Reward }

        let blockChain = (blockChain, transaction) |> addTransaction
        blockChain

    let isValid blockChain =
        let mutable result = false
        for index in 1..blockChain.Chain |> List.length do
            let currentBlock = blockChain.Chain.[index]
            let previousBlock = blockChain.Chain.[index - 1]
            match (currentBlock.Hash <> (currentBlock |> Block.calculateHash),
                   currentBlock.PreviousHash <> previousBlock.Hash) with
            | false, false -> result <- false
            | false, _ -> result <- false
            | _, false -> result <- false
            | _ -> result <- true
        result

    let getBalance (blockChain, address) =
        let transactions =
            blockChain.Chain
            |> List.map (fun block -> block.Transactions)
            |> List.fold (fun acc value -> acc @ value) []

        let debitTransactionsAmount =
            transactions
            |> List.where
                   (fun transaction -> transaction.SenderAddress = address)
            |> List.sumBy (fun transaction -> transaction.Amount)

        let creditTransactionsAmount =
            transactions
            |> List.where
                   (fun transaction -> transaction.RecipientAddress = address)
            |> List.sumBy (fun transaction -> transaction.Amount)

        creditTransactionsAmount - debitTransactionsAmount
