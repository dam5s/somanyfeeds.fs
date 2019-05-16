module SoManyFeedsServer.ReadPage

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.FeedsDataGateway
open Suave
open Suave.DotLiquid


type FrontendPage =
    | Recent of feedId : int64 option
    | Bookmarks


type private Flags =
    { UserName : string
      Recents : ArticleRecord seq
      Feeds : FeedRecord seq
      Page : FrontendPage
    }


type ReadViewModel =
    { Flags : string
    }


module private Encoders =
    open Chiron
    open Chiron.Operators

    let articlesEncoder (feeds : FeedRecord seq) (articles : ArticleRecord seq) : Json =
        articles
        |> Seq.map (Json.serializeWith (ArticlesApi.Encoders.article feeds))
        |> Seq.toList
        |> Json.Array

    let feedsEncoder (feeds : FeedRecord seq) : Json =
        feeds
        |> Seq.map (Json.serializeWith FeedsApi.Encoders.feed)
        |> Seq.toList
        |> Json.Array

    let maybeFeedId (page : FrontendPage) =
        match page with
        | Recent maybeFeedId -> maybeFeedId
        | Bookmarks -> None

    let pageJson (page : FrontendPage) =
        match page with
        | Recent _ -> "Recent"
        | Bookmarks -> "Bookmarks"

    let flags (flags : Flags) : Json<unit> =
        Json.write "userName" flags.UserName
        *> Json.writeWith (articlesEncoder flags.Feeds) "recents" flags.Recents
        *> Json.writeWith feedsEncoder "feeds" flags.Feeds
        *> Json.write "page" (pageJson flags.Page)
        *> Json.write "selectedFeedId" (maybeFeedId flags.Page)


let page
    (listFeedsAndArticles : Authentication.User -> int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user : Authentication.User)
    (frontendPage : FrontendPage) : WebPart =

    fun ctx -> async {
        let maybeFeedId = Encoders.maybeFeedId frontendPage
        let! listResult = listFeedsAndArticles user maybeFeedId

        match listResult with
        | Ok (feeds, articles) ->
            let flags =
                { UserName = user.Name
                  Recents = articles
                  Feeds = feeds
                  Page = frontendPage
                }
            let viewModel =
                { Flags = Json.serializeObject Encoders.flags flags }

            return! page "read.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
