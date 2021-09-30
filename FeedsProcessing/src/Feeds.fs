module FeedsProcessing.Feeds

open FeedsProcessing.Download


type FeedName = FeedName of string

type TwitterHandle = TwitterHandle of string

type Feed =
    | Xml of FeedName * Url
    | Twitter of TwitterHandle
