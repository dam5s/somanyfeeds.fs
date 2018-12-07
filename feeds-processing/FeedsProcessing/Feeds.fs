module FeedsProcessing.Feeds


type FeedName = FeedName of string

let feedNameValue (FeedName u) = u


type FeedUrl = FeedUrl of string

let feedUrlValue (FeedUrl u) = u


type TwitterHandle = TwitterHandle of string

let twitterHandleValue (TwitterHandle s) = s


type Feed =
    | Rss of FeedName * FeedUrl
    | Atom of FeedName * FeedUrl
    | Twitter of TwitterHandle
