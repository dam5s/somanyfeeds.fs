module Server.Url


type Url = Url of string

let urlString (Url u) = u
