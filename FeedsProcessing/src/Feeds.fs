module FeedsProcessing.Feeds

open FeedsProcessing.Download


type FeedName = FeedName of string

type Feed =
    | Xml of FeedName * Url
