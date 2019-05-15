module SoManyFeedsServer.ReadPage

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open Suave
open Suave.DotLiquid


type ReadViewModel =
    { UserName : string
      RecentsJson : string
      FeedsJson : string
      Page : string
      SelectedFeedId : string
    }


type FrontendPage =
    | Recent of feedId : int64 option
    | Bookmarks


let private maybeFeedId (page : FrontendPage) =
    match page with
    | Recent maybeFeedId -> maybeFeedId
    | Bookmarks -> None


let private pageJson (page : FrontendPage) =
    match page with
    | Recent _ -> "Recent"
    | Bookmarks -> "Bookmarks"


let private feedIdJson (maybeFeedId : int64 option) =
    maybeFeedId
   |> Option.map (sprintf "%d")
   |> Option.defaultValue "null"




let page
    (listFeedsAndArticles : Authentication.User -> int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user : Authentication.User)
    (frontendPage : FrontendPage) : WebPart =

    fun ctx -> async {
        let maybeFeedId = maybeFeedId frontendPage
        let! listResult = listFeedsAndArticles user maybeFeedId

        match listResult with
        | Ok (feeds, articles) ->
            let viewModel =
                { UserName = user.Name
                  RecentsJson = Json.serializeList (ArticlesApi.Encoders.article feeds) articles
                  FeedsJson = Json.serializeList FeedsApi.Encoders.feed feeds
                  Page = pageJson frontendPage
                  SelectedFeedId = feedIdJson maybeFeedId
                }
            return! page "read.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
