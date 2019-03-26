module SoManyFeedsServer.ArticlesApi

open Time
open Suave
open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open SoManyFeedsServer.Json


module Encoders =
    open Chiron
    open Chiron.Operators

    let article (feedOption : FeedRecord option, article : ArticleRecord) : Json<unit> =
        let feedName = feedOption
                       |> Option.map (fun f -> f.Name)
                       |> Option.defaultValue ""

        Json.write "feedName" feedName
        *> Json.write "url" article.Url
        *> Json.write "title" article.Title
        *> Json.write "feedUrl" article.FeedUrl
        *> Json.write "content" article.Content
        *> Json.write "date" (Option.map Posix.milliseconds article.Date)
        *> Json.write "readUrl" (sprintf "/api/articles/%d/read" article.Id)


let list (listArticles : AsyncResult<(FeedRecord option * ArticleRecord) seq>) : WebPart =
    fun ctx -> async {
        let! articlesResult = listArticles

        match articlesResult with
        | Ok articles -> return! listResponse HTTP_200 Encoders.article articles ctx
        | Error message -> return! serverErrorResponse message ctx
    }

let update (updateOperation : AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        let! updateResult = updateOperation

        match updateResult with
        | Ok _ -> return! jsonResponse HTTP_200 "" ctx
        | Error message -> return! serverErrorResponse message ctx
    }
