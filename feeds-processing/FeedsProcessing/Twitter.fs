module FeedsProcessing.Twitter


open FSharp.Data
open FeedsProcessing.Article

open FeedsProcessing.Download
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open System
open System.Globalization


type private Tweet =
    { Text: string
      CreatedAt: DateTime option
      IsRetweet: bool
      IsReply: bool
    }


let private parseDate dateValue =
    unsafeOperation "Date parsing" { return fun _ ->
        DateTime.ParseExact(dateValue, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture)
    }
    |> Result.toOption


type TwitterTimelineProvider = JsonProvider<"../feeds-processing/Resources/samples/twitter.timeline.sample.json">


let private mapTweet (json: TwitterTimelineProvider.Root) =
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


let private parseTweets (download: Download) =
    unsafeOperation "Parse tweets" { return fun _ ->
        download.Content
        |> TwitterTimelineProvider.Parse
        |> Array.toList
        |> List.map mapTweet
    }


let private tweetToArticle (TwitterHandle handle) tweet =
    Article.create
        None
        $"https://twitter.com/%s{handle}"
        (Some tweet.Text)
        (tweet.CreatedAt |> Option.map DateTimeOffset)



let private tweetsToArticles handle tweets =
    tweets
    |> List.filter (fun t -> not t.IsRetweet && not t.IsReply)
    |> List.map (tweetToArticle handle)


let processTweets handle downloaded: ProcessingResult =
    Result.map (tweetsToArticles handle) (parseTweets downloaded)
