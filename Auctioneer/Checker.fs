namespace Auctioneer

type Realm = string
type InsertionTime = int64

module Option =

    let apply (a : 'a option) (f : ('a -> 'b) option) =
        match f with
        | None -> None
        | Some f -> Option.map f a

module InsertionCheck =

    type private InsertionMessage =
        | FetchLastInsertion
        | IsValidInsertion of (Realm * InsertionTime * AsyncReplyChannel<bool>)
        | UpdateLastInsertion of (Realm * InsertionTime)

    let private agent =
        MailboxProcessor<InsertionMessage>.Start(fun inbox ->
            let rec loop (lastInsert : Map<Realm, InsertionTime>) =
                async {
                    let! message = inbox.Receive ()
                    match message with
                    | FetchLastInsertion ->
                        //Go to the database, grab the last insert for each realm
                        return! loop lastInsert
                    | IsValidInsertion (realm, currentInsert, channel) ->
                        match Map.tryFind realm lastInsert with
                        | None -> channel.Reply true
                        | Some v ->
                            v < currentInsert
                            |> channel.Reply
                        return! loop lastInsert
                    | UpdateLastInsertion (realm, currentInsert) ->
                        //Push this insert into the database, if it fails -> add a message to retry
                        match Map.tryFind realm lastInsert with
                        | None ->
                            return! loop <| Map.add realm currentInsert lastInsert
                        | Some v ->
                            return! loop <| Map.add realm (max v currentInsert) lastInsert
                }
            loop Map.empty
        )

    let fetchLatest () =
        agent.Post FetchLastInsertion

    let isNewInsertion (realm : Realm) (modificationTime : InsertionTime) =
        agent.PostAndAsyncReply (fun c -> IsValidInsertion (realm, modificationTime, c))

    let updateLatestInsertion (realm : Realm) (modificationTime : InsertionTime) =
        agent.Post <| UpdateLastInsertion (realm, modificationTime)
