module Server.Feeds

open Server.Url
open Server.SourceType

type TwitterHandle = TwitterHandle of string

type Feed =
    | Rss of SourceType * Url
    | Atom of SourceType * Url
    | Twitter of TwitterHandle


module Repository =

    let findAll (): Feed list = [
        Atom (Code, Url "https://github.com/dam5s.atom")
        Rss (Blog, Url "https://medium.com/feed/@its_damo")
        Twitter (TwitterHandle "its_damo")
    ]
