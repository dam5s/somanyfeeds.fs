[<RequireQualifiedAccess>]
module SoManyFeedsServer.ReadPage

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsDomain.User


type FrontendPage =
    | Recent of feedId: int64 option
    | Bookmarks


type private Flags =
    { UserName: string
      Recents: ArticleRecord seq
      Feeds: FeedRecord seq
      Page: FrontendPage }


module private Json =
    let private articleList feeds articles =
        articles
        |> Seq.map (ArticlesApi.Json.article feeds)
        |> Seq.toList

    let private feedList feeds =
        feeds
        |> Seq.map FeedsApi.Json.feed
        |> Seq.toList

    let private pageJson page =
        match page with
        | Recent _ -> "Recent"
        | Bookmarks -> "Bookmarks"

    let maybeFeedId page =
        match page with
        | Recent maybeFeedId -> maybeFeedId
        | Bookmarks -> None

    let flags flags =
        {| userName = flags.UserName
           recents = articleList flags.Feeds flags.Recents
           feeds = feedList flags.Feeds
           page = pageJson flags.Page
           selectedFeedId = maybeFeedId flags.Page |}


module private View =
    let render (flagsJson: string) =
        flagsJson
        |> sprintf "SoManyFeeds.StartReadApp(%s);"
        |> Layout.startFableApp


let page
    (listFeedsAndArticles: int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user: User)
    (frontendPage: FrontendPage): HttpHandler =

    fun next ctx ->
        task {
            let maybeFeedId = Json.maybeFeedId frontendPage

            match! listFeedsAndArticles maybeFeedId with
            | Ok(feeds, articles) ->
                let flags =
                    { UserName = user.Name
                      Recents = articles
                      Feeds = feeds
                      Page = frontendPage }
                let flagsJson = Api.serializeObject (Json.flags flags) ctx
                return! htmlView (View.render flagsJson) next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
