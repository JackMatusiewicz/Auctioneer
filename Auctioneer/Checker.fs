namespace Auctioneer

module Option =

    let apply (a : 'a option) (f : ('a -> 'b) option) =
        match f with
        | None -> None
        | Some f -> Option.map f a

module InsertionCheck =

    type private InsertionMessage =
        | FetchLastInsertion
        | IsValidInsertion of (int64 * AsyncReplyChannel<bool>)
        | UpdateLastInsertion of int64

    let private agent =
        MailboxProcessor<InsertionMessage>.Start(fun inbox ->
            let rec loop (lastInsert : int64 option) =
                async {
                    let! message = inbox.Receive ()
                    match message with
                    | FetchLastInsertion ->
                        //Go to the database, grab the last insert
                        return! loop lastInsert
                    | IsValidInsertion (currentInsert, channel) ->
                        match lastInsert with
                        | None -> channel.Reply true
                        | Some v ->
                            v < currentInsert
                            |> channel.Reply
                        return! loop lastInsert
                    | UpdateLastInsertion currentInsert ->
                        //Push this insert into the database, if it fails -> add a message to retry
                        let v =
                            Option.map max lastInsert
                            |> Option.apply (Some currentInsert)
                            |> Option.defaultValue currentInsert
                        return! loop (Some v)
                }
            loop None
        )

    let fetchLatest () =
        agent.Post FetchLastInsertion

    let isNewInsertion (modificationTime : int64) =
        agent.PostAndAsyncReply (fun c -> IsValidInsertion (modificationTime, c))

    let updateLatestInsertion (modificationTime : int64) =
        agent.Post <| UpdateLastInsertion modificationTime
