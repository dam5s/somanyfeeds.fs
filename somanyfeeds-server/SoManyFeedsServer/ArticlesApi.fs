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

    let article (feed : FeedRecord, article : ArticleRecord) : Json<unit> =
        Json.write "feedName" feed.Name
        *> Json.write "url" article.Url
        *> Json.write "title" article.Title
        *> Json.write "feedUrl" article.FeedUrl
        *> Json.write "content" article.Content
        *> Json.write "date" (Option.map dateMap article.Date)


let list (listArticles : AsyncResult<(FeedRecord * ArticleRecord) list>) : WebPart =
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
