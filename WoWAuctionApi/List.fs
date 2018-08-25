namespace WoWAuctionApi

module List =

    let private append (x : 'a) (xs : 'a list) =
        x :: xs

    let rec traverseAsync (f : 'a -> 'b Async) (xs : 'a list) : Async<'b list> =
        async {
            match xs with
            | [] -> return []
            | head::tail ->
                let! h = f head
                let! tail = traverseAsync f tail
                return h :: tail
        }