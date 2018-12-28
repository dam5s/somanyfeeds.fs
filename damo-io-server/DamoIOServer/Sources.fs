module DamoIOServer.Sources

open FeedsProcessing.Feeds


type SourceType =
    | About
    | Social
    | Code
    | Blog


type Source = SourceType * Feed


module Repository =

    let findAll (): Source list = [
        (Code, Xml (FeedName "Github", FeedUrl "https://github.com/dam5s.atom"))
        (Blog, Xml (FeedName "Medium", FeedUrl "https://medium.com/feed/@its_damo"))
        (Social, (Twitter (TwitterHandle "dam5s")))
    ]
