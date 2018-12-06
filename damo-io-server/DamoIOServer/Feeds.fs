module DamoIOServer.Feeds

open DamoIOServer.FeedUrl
open DamoIOServer.SourceType

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
        Twitter (TwitterHandle "dam5s")
    ]
