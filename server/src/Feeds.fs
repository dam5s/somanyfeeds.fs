module Server.Feeds

open Server.FeedUrl
open Server.SourceType

type TwitterHandle = TwitterHandle of string

let twitterHandleValue (TwitterHandle s) = s

type Feed =
    | Rss of SourceType * FeedUrl
    | Atom of SourceType * FeedUrl
    | Twitter of TwitterHandle


module Repository =

    let findAll (): Feed list = [
        Atom (Code, FeedUrl "https://github.com/dam5s.atom")
        Rss (Blog, FeedUrl "https://medium.com/feed/@its_damo")
        Twitter (TwitterHandle "its_damo")
    ]
