module SoManyFeeds.Processors.Rss

open System
open SoManyFeeds.ArticlesProcessing
open SoManyFeeds.ArticlesData
open SoManyFeeds.Feeds

let private dummyArticle : Record =
    { Title = Some "Hello world"
      Link = None
      Content = "article content"
      Date = Some DateTime.Now
      Source = "social" }


let processFeed (feed : Feed) : ProcessingResult =
    match feed.Type with
    | Rss -> ProcessingResult.Ok [ dummyArticle ]
    | _  -> ProcessingResult.CannotProcess
