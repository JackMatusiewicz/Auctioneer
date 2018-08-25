namespace WoWAuctionApi

open Newtonsoft.Json
open FSharp.Data

module Auction =

    let private downloadAuctionFile fileUrl =
        async {
            let! data = Http.AsyncRequestString fileUrl
            return JsonConvert.DeserializeObject<_> data
        }

    let getFilesForRealm
        (key : string)
        (realm : string)
        : Async<AuctionApiResponseDto>
        =
        async {
            let! data =
                Http.AsyncRequestString (
                    sprintf "https://eu.api.battle.net/wow/auction/data/%s" realm,
                    httpMethod= "GET",
                    query= [("locale", "en_GB"); ("apikey", key)]
                )
            return JsonConvert.DeserializeObject<_> data
        }

    let getAuctionFiles key realm =
        async {
            let! resp = getFilesForRealm key realm
            return!
                List.traverseAsync
                    (fun d -> downloadAuctionFile d.Url)
                    resp.Files
        }