namespace WoWAuctionApi

open Newtonsoft.Json

type AuctionDto = {
    [<JsonProperty("auc")>]
    AuctionId : int64

    [<JsonProperty("item")>]
    ItemId : int64

    [<JsonProperty("owner")>]
    Seller : string

    [<JsonProperty("ownerRealm")>]
    SellerRealm : string

    [<JsonProperty("bid")>]
    Bid : int64

    [<JsonProperty("buyout")>]
    Buyout : int64

    [<JsonProperty("quantity")>]
    Quantity : int

    [<JsonProperty("timeLeft")>]
    TimeRemaining : string

    [<JsonProperty("rand")>]
    Rand : int64

    [<JsonProperty("seed")>]
    Seed : int64

    [<JsonProperty("context")>]
    Context : int
}

type RealmDto = {
    [<JsonProperty("name")>]
    Name : string

    [<JsonProperty("slug")>]
    Slug : string
}

type AuctionDataDto = {
    [<JsonProperty("realms")>]
    Realms : RealmDto list

    [<JsonProperty("auctions")>]
    Auctions : AuctionDto list
}

type TimeStampedAuctionDataDto = {
    Data : AuctionDataDto
    Collected : int64
}

type AuctionFileDto = {
    [<JsonProperty("url")>]
    Url : string

    [<JsonProperty("lastModified")>]
    TimeStamp : int64
}

type AuctionApiResponseDto = {
    [<JsonProperty("files")>]
    Files : AuctionFileDto list
}