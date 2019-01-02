module SoManyFeedsServer.ReadPage

open SoManyFeedsServer.ArticlesPersistence
open SoManyFeedsServer.FeedsPersistence
open Suave
open Suave.DotLiquid


type ReadViewModel =
    { UserName : string
      ArticlesJson : string
    }


let page (listArticles : unit -> Result<(FeedRecord * ArticleRecord) list, string>) (user : Authentication.User) : WebPart =
    match listArticles () with
    | Ok records ->
        let viewModel =
            { UserName = user.Name
              ArticlesJson = Json.serializeList ArticlesApi.Encoders.article records
            }
        page "read.html.liquid" viewModel
    | Error message ->
        ErrorPage.page message
