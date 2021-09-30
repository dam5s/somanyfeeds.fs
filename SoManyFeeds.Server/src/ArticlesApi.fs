[<RequireQualifiedAccess>]
module SoManyFeedsServer.ArticlesApi

open Giraffe
open Time
open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsFrontend.Components.Article


module Json =
    let article (feeds: FeedRecord seq) (article: ArticleRecord): Article.Json =
        let feedName =
            feeds
            |> Seq.tryFind (fun f -> f.Url = article.FeedUrl)
            |> Option.map (fun f -> f.Name)
            |> Option.defaultValue ""

        { feedName = feedName
          url = article.Url
          title = article.Title
          feedUrl = article.FeedUrl
          content = article.Content
          date = (Option.map Posix.milliseconds article.Date)
          readUrl = (sprintf "/api/articles/%d/read" article.Id)
          bookmarkUrl = (sprintf "/api/articles/%d/bookmark" article.Id) }

let list (listArticles: AsyncResult<FeedRecord seq * ArticleRecord seq>): HttpHandler =
    let jsonMapping (feeds, articles) =
        Seq.map (Json.article feeds) articles

    listArticles |> Api.view jsonMapping

let update (updateOperation: AsyncResult<unit>): HttpHandler =
    updateOperation |> Api.action
