open WoWAuctionApi

[<EntryPoint>]
let main argv =
    Auction.getAuctionFiles "TODO" "deathwing"
    |> Async.RunSynchronously
    |> List.map (fun ad -> List.length ad.Auctions)
    |> List.iter (printfn "%d")
    0
