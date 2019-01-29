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
        *> Json.write "markReadUrl" (sprintf "/api/articles/%d/mark-read" article.Id)
        *> Json.write "markUnreadUrl" (sprintf "/api/articles/%d/mark-unread" article.Id)


let list (listArticles : AsyncResult<(FeedRecord option * ArticleRecord) list>) : WebPart =
    fun ctx -> async {
        match! listArticles with
        | Ok articles ->
            return! articles
            |> serializeList Encoders.article
            |> jsonResponse HTTP_200
            |> fun wp -> wp ctx
        | Error message ->
            return! serverError message ctx
    }

let update (updateOperation : AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        match! updateOperation with
        | Ok _ ->
            return! ""
            |> jsonResponse HTTP_200
            |> fun wp -> wp ctx
        | Error message ->
            return! serverError message ctx
    }
