[<RequireQualifiedAccess>]
module String

let isEmpty value =
    match value with
    | "" -> true
    | _ -> false

let contains (subString: string) (it: string) = it.Contains(subString)

let equals other (it: string) = it.Equals(other)

let trim (it: string) = it.Trim()

let toLowerInvariant (it: string) = it.ToLowerInvariant()
