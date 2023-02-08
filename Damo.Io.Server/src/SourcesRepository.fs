[<RequireQualifiedAccess>]
module DamoIoServer.SourcesRepository

open DamoIoServer.Source
open FeedsProcessing.Download
open FeedsProcessing.Feeds

type SourceFeed = Source * Feed

let findAll (): SourceFeed list =
    [ (Code, Xml(FeedName "Github", Url "https://github.com/dam5s.atom"))
      (Blog, Xml(FeedName "Blog", Url "https://blog.damo.io/rss.xml"))
      (Social, Xml(FeedName "Mastodon", Url "https://mastodon.kleph.eu/users/dam5s.rss"))
    ]
