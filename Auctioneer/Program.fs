
open WoWAuctionApi
open System

[<EntryPoint>]
let main argv =
    Auction.getAuctionFiles "TODO" "deathwing"
    |> Async.RunSynchronously
    |> List.map (fun ad -> List.length ad.Auctions)
    |> List.iter (printfn "%d")
    0 // return an integer exit code
