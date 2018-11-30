module Server.FeedUrl


type FeedUrl = FeedUrl of string

let feedUrlString (FeedUrl u) = u
