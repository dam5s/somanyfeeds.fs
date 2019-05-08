module SoManyFeedsServer.ReadPage

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open Suave
open Suave.DotLiquid


type ReadViewModel =
    { UserName : string
      ArticlesJson : string
      FeedsJson : string
      SelectedFeedId : string
    }


let page
    (listFeedsAndArticles : Authentication.User -> int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user : Authentication.User)
    (maybeFeedId : int64 option) : WebPart =

    fun ctx -> async {
        let! listResult = listFeedsAndArticles user maybeFeedId

        match listResult with
        | Ok (feeds, articles) ->
            let viewModel =
                { UserName = user.Name
                  ArticlesJson = Json.serializeList (ArticlesApi.Encoders.article feeds) articles
                  FeedsJson = Json.serializeList FeedsApi.Encoders.feed feeds
                  SelectedFeedId = maybeFeedId
                                   |> Option.map (sprintf "%d")
                                   |> Option.defaultValue "null"
                }
            return! page "read.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
