module DamoIOServer.Sources

open FeedsProcessing.Download
open FeedsProcessing.Feeds


type SourceType =
    | About
    | Social
    | Code
    | Blog


type Source = SourceType * Feed


module Repository =

    let findAll(): Source list =
        [ (Code, Xml(FeedName "Github", Url "https://github.com/dam5s.atom"))
          (Blog, Xml(FeedName "Blog", Url "https://blog.damo.io/rss.xml"))
          (Social, (Twitter(TwitterHandle "dam5s")))
        ]
