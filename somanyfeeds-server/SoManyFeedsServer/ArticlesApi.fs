module SoManyFeedsServer.ArticlesApi

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open SoManyFeedsServer.Json
open Suave


module Encoders =
    open System
    open Chiron
    open Chiron.Operators

    let private epoch: DateTime =
        new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

    let private dateMap (d : DateTimeOffset): int64 =
        d.ToUnixTimeMilliseconds ()

    let article (feedOption : FeedRecord option, article : ArticleRecord) : Json<unit> =
        let feedName = feedOption
                       |> Option.map (fun f -> f.Name)
                       |> Option.defaultValue ""

        Json.write "feedName" feedName
        *> Json.write "url" article.Url
        *> Json.write "title" article.Title
        *> Json.write "feedUrl" article.FeedUrl
        *> Json.write "content" article.Content
        *> Json.write "date" (Option.map dateMap article.Date)
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
