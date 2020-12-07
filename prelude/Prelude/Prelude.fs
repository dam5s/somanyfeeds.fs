[<AutoOpen>]
module Prelude

let always a _ = a

let curry f a b = f (a, b)

let curry2 f a b c = f (a, b, c)

let tryCast<'a> (a: obj) =
    try Some (a :?> 'a)
    with | _ -> None
