module FableFrontend.Support.Posix

open Time


let private monthDisplayName month =
    match month with
    | 1 -> "January"
    | 2 -> "February"
    | 3 -> "March"
    | 4 -> "April"
    | 5 -> "May"
    | 6 -> "June"
    | 7 -> "July"
    | 8 -> "August"
    | 9 -> "September"
    | 10  -> "October"
    | 11 -> "November"
    | 12 -> "December"
    | x -> failwithf "Not a valid month rank: %i" x

let toString posix =
    let date = Posix.toDateTime posix
    let month = monthDisplayName date.Month

    sprintf "%s %s" month (date.ToString "dd 'yy @ HH:mm")
