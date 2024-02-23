module DamoIoServer.SourcesRepository

open DamoIoServer.Source
open FeedsProcessing.Download
open FeedsProcessing.Feeds

type SourceFeed =
    { Type: Source
      Name: string
      Feed: Feed }

[<RequireQualifiedAccess>]
module SourcesRepository =

    let findAll () : SourceFeed list =
        [ { Type = Code
            Name = "Github"
            Feed = Xml(Url "https://github.com/dam5s.atom") }
          { Type = Blog
            Name = "Blog"
            Feed = Xml(Url "https://blog.damo.io/rss.xml") }
          { Type = Social
            Name = "Mastodon"
            Feed = Xml(Url "https://mastodon.kleph.eu/users/dam5s.rss") }
          { Type = Social
            Name = "Bluesky"
            Feed = Xml(Url "https://bsky.app/profile/did:plc:zvnvcicnso363xz3gu6ho3mw/rss") } ]
