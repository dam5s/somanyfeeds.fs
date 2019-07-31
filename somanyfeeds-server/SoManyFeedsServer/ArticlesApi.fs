module SoManyFeedsServer.ArticlesApi

open Time
open Suave
open SoManyFeeds.ArticlesDataGateway
open SoManyFeeds.FeedsDataGateway
open SoManyFeedsServer.Json


module Encoders =
    open Chiron
    open Chiron.Operators

    let article (feeds : FeedRecord seq) (article : ArticleRecord) : Json<unit> =
        let feedName = feeds
                       |> Seq.tryFind (fun f -> f.Url = article.FeedUrl)
                       |> Option.map (fun f -> f.Name)
                       |> Option.defaultValue ""

        Json.write "feedName" feedName
        *> Json.write "url" article.Url
        *> Json.write "title" article.Title
        *> Json.write "feedUrl" article.FeedUrl
        *> Json.write "content" article.Content
        *> Json.write "date" (Option.map Posix.milliseconds article.Date)
        *> Json.write "readUrl" (sprintf "/api/articles/%d/read" article.Id)
        *> Json.write "bookmarkUrl" (sprintf "/api/articles/%d/bookmark" article.Id)


let list (listArticles : AsyncResult<FeedRecord seq * ArticleRecord seq>) : WebPart =
    fun ctx -> async {
        match! listArticles with
        | Ok (feeds, articles) -> return! listResponse HTTP_200 (Encoders.article feeds) articles ctx
        | Error message -> return! serverErrorResponse message ctx
    }

let update (updateOperation : AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        match! updateOperation with
        | Ok _ -> return! jsonResponse HTTP_200 "" ctx
        | Error message -> return! serverErrorResponse message ctx
    }
