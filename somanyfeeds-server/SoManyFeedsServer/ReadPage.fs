module SoManyFeedsServer.ReadPage

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open Suave
open Suave.DotLiquid


type ReadViewModel =
    { UserName : string
      ArticlesJson : string
    }


let page (listArticles : AsyncResult<(FeedRecord option * ArticleRecord) seq>) (user : Authentication.User) : WebPart =
    fun ctx -> async {
        let! listResult = listArticles

        match listResult with
        | Ok records ->
            let viewModel =
                { UserName = user.Name
                  ArticlesJson = Json.serializeList ArticlesApi.Encoders.article records
                }
            return! page "read.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
