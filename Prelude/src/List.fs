[<RequireQualifiedAccess>]
module List

let all predicate list =
    let filtered = List.filter predicate list
    List.length filtered = List.length list

let updateIf predicate updateFunction =
    List.map (fun elt -> if predicate elt then updateFunction elt else elt)

let updateOne element updateFunction =
    List.map (fun elt -> if elt = element then updateFunction elt else elt)
