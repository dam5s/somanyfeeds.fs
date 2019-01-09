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
    tryOperation
        "Date parsing"
        (fun _ -> DateTime.ParseExact (dateValue, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture))

    |> Result.toOption


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
    tryOperation "Parse tweets" (fun _ ->
        downloaded
        |> TwitterTimelineProvider.Parse
        |> Array.toList
        |> List.map mapTweet
    )


let private tweetToArticle (TwitterHandle handle) (tweet : Tweet) : Article =
    { Title = None
      Link = Some <| sprintf "https://twitter.com/%s" handle
      Content = tweet.Text
      Date = tweet.CreatedAt |> Option.map (fun d -> new DateTimeOffset (d))
    }


let private tweetsToArticles (handle : TwitterHandle) (tweets : Tweet list) : Article list =
    tweets
    |> List.filter (fun t -> not t.IsRetweet && not t.IsReply)
    |> List.map (tweetToArticle handle)


let processTweets (handle : TwitterHandle) (downloaded : DownloadedFeed) : ProcessingResult =
    Result.map (tweetsToArticles handle) (parseTweets downloaded)
