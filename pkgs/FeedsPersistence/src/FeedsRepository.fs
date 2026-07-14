module FeedsPersistence.FeedsRepository

open System.Threading.Tasks

open FeedsProcessing.Download
open FeedsProcessing.Feeds

type FeedRecord = { Name: string; Feed: Feed }

type FeedsRepository() =
    let feeds =
        [ { Name = "Github"
            Feed = Xml(Url "https://github.com/dam5s.atom") }
          { Name = "Blog"
            Feed = Xml(Url "https://blog.damo.io/rss.xml") }
          { Name = "Mastodon"
            Feed = Xml(Url "https://mastodon.kleph.eu/users/dam5s.rss") }
          { Name = "Bluesky"
            Feed = Xml(Url "https://bsky.app/profile/did:plc:zvnvcicnso363xz3gu6ho3mw/rss") } ]

    member _.FindAllAsync() : Task<FeedRecord list> = task { return feeds }
