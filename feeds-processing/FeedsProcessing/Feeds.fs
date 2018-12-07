module FeedsProcessing.Feeds


type FeedName = FeedName of string

type FeedUrl = FeedUrl of string

type TwitterHandle = TwitterHandle of string

type Feed =
    | Rss of FeedName * FeedUrl
    | Atom of FeedName * FeedUrl
    | Twitter of TwitterHandle
