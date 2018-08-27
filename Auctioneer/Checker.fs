namespace Auctioneer

type Realm = string
type InsertionTime = int64
type 'a Agent = 'a MailboxProcessor

type IInsertionChecker =
    abstract member FetchLatest : unit -> unit
    abstract member IsNewInsertion : Realm -> InsertionTime -> Async<bool>
    abstract member UpdateLatestInsertion : Realm -> InsertionTime -> unit

module InsertionCheck =

    type internal InsertionMessage =
        | FetchLastInsertion
        | IsValidInsertion of (Realm * InsertionTime * AsyncReplyChannel<bool>)
        | UpdateLastInsertion of (Realm * InsertionTime)

    let internal makeAgent sqlConnectionString = Agent<_>.Start(fun inbox ->
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

    let internal fetchLatest (agent : Agent<_>) conStr =
        agent.Post FetchLastInsertion

    let internal isNewInsertion
        (agent : Agent<_>)
        (realm : Realm)
        (modificationTime : InsertionTime)
        =
        agent.PostAndAsyncReply (fun c -> IsValidInsertion (realm, modificationTime, c))

    let internal updateLatestInsertion
        (agent : Agent<_>)
        (realm : Realm)
        (modificationTime : InsertionTime)
        =
        agent.Post <| UpdateLastInsertion (realm, modificationTime)

    let make (connectionString : string) =
        let agent = makeAgent connectionString

        { new IInsertionChecker with
            member __.FetchLatest () =
                fetchLatest agent ()
            member __.IsNewInsertion r i =
                isNewInsertion agent r i
            member __.UpdateLatestInsertion r i =
                updateLatestInsertion agent r i
        }
