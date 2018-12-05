module Server.FeedsProcessing.Twitter


open System
open FSharp.Data

open Server.SourceType
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.Feeds
open Server.Articles.Data
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


type TwitterTimelineProvider = JsonProvider<"../damo-io-server/resources/samples/twitter.timeline.sample">


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


let private parseTweets (downloaded : DownloadedFeed) : Result<Tweet list, string> =
    try
        downloadedString downloaded
            |> TwitterTimelineProvider.Parse
            |> Array.toList
            |> List.map mapTweet
            |> Ok
    with
    | ex ->
        printfn "Could not parse tweets json\n\n%s\n\nGot exception %s" (downloadedString downloaded) (ex.ToString ())
        Error "Could not parse tweets json"


let private tweetToArticle (handle : TwitterHandle) (tweet : Tweet) : Record =
    { Title = None
      Link = Some <| String.Format ("https://twitter.com/{0}", twitterHandleValue handle)
      Content = tweet.Text
      Date = tweet.CreatedAt
      Source = Social
    }


let private tweetsToArticles (handle : TwitterHandle) (tweets : Tweet list) : Record list =
    tweets
        |> List.filter (fun t -> not t.IsRetweet && not t.IsReply)
        |> List.map (tweetToArticle handle)


let processTweets (handle : TwitterHandle) (downloaded : DownloadedFeed) : ProcessingResult =
    Result.map (tweetsToArticles handle) (parseTweets downloaded)
