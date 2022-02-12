[<AutoOpen>]
module TryCast

let tryCast<'a> (a: obj) =
    try Some (a :?> 'a)
    with | _ -> None
