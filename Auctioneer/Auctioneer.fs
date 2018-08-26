namespace Auctioneer

type 'a Agent = 'a MailboxProcessor

module Downloader =

    type private DownloadMessage =
        | AddRealm of string
        | DownloadAuctions

    let private agent = Agent<_>.Start(fun inbox ->
        let rec loop (realms : Set<string>) =
            async {
                let! message = inbox.Receive ()
                match message with
                | AddRealm realm ->
                    return! loop <| Set.add realm realms
                | DownloadAuctions ->
                    //Go through each realm, download auction data, stash in DB.
                    return! loop realms
            }
        loop Set.empty
    )

    let add realm =
        agent.Post (AddRealm realm)

    let downloadLatest () =
        agent.Post DownloadAuctions
