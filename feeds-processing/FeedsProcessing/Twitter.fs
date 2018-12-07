module FeedsProcessing.Twitter


open System
open FSharp.Data

open FeedsProcessing.Article
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Download
open FeedsProcessing.Feeds
open System.Globalization


type private Tweet =
    { Text : string
      CreatedAt : DateTime option
      IsRetweet : bool
      IsReply : bool
    }


let private parseDate (dateValue : string) : DateTime option =
    try
        Some <| DateTime.ParseExact (dateValue, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture)
    with
    | ex ->
        printfn "There was an error parsing the date. %s" (ex.ToString ())
        None


type TwitterTimelineProvider = JsonProvider<"../feeds-processing/Resources/samples/twitter.timeline.sample">


let private mapTweet (json : TwitterTimelineProvider.Root) : Tweet =
    let createdAtString = json.CreatedAt
    let replyToScreenName = json.InReplyToScreenName
    let retweetedStatus = json.RetweetedStatus

    { Text = json.Text
      CreatedAt = parseDate createdAtString
      IsRetweet = Option.isSome retweetedStatus
      IsReply = replyToScreenName
      |> Option.map String.IsNullOrEmpty
      |> Option.defaultValue true
      |> not
    }


let private parseTweets (DownloadedFeed downloaded) : Result<Tweet list, string> =
    try
        downloaded
            |> TwitterTimelineProvider.Parse
            |> Array.toList
            |> List.map mapTweet
            |> Ok
    with
    | ex ->
        printfn "Could not parse tweets json\n\n%s\n\nGot exception %s" downloaded (ex.ToString ())
        Error "Could not parse tweets json"


let private tweetToArticle (TwitterHandle handle) (tweet : Tweet) : Article =
    { Title = None
      Link = Some <| String.Format ("https://twitter.com/{0}", handle)
      Content = tweet.Text
      Date = tweet.CreatedAt |> Option.map (fun d -> new DateTimeOffset(d))
    }


let private tweetsToArticles (handle : TwitterHandle) (tweets : Tweet list) : Article list =
    tweets
        |> List.filter (fun t -> not t.IsRetweet && not t.IsReply)
        |> List.map (tweetToArticle handle)


let processTweets (handle : TwitterHandle) (downloaded : DownloadedFeed) : ProcessingResult =
    Result.map (tweetsToArticles handle) (parseTweets downloaded)
