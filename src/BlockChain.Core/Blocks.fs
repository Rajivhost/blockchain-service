namespace BlockChain

open System
open System.Text
open System.Security.Cryptography
open Newtonsoft.Json

type Transaction =
    { SenderAddress : string
      RecipientAddress : string
      Amount : decimal }

type Block =
    { Index : int
      TimeStamp : DateTime
      PreviousHash : string
      Hash : string
      Transactions : Transaction list
      Nonce : int }

module Block =
    let empty =
        { Index = 0
          TimeStamp = DateTime.Now
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

        while Object.ReferenceEquals(hash, null) || hash.Substring(0, difficulty) <> leadingZeros do
            nonce <- nonce + 1
            hash <- { block with Nonce = nonce; Hash = hash } |> calculateHash

        { block with Nonce = nonce; Hash = hash }
