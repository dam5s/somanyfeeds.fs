[<RequireQualifiedAccess>]
module SoManyFeedsServer.ReadPage

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsFrontend.Applications
open SoManyFeedsDomain.User


module private Json =
    let articleArray feeds articles =
        articles
        |> Seq.map (ArticlesApi.Json.article feeds)
        |> Seq.toArray

    let feedArray feeds =
        feeds
        |> Seq.map FeedsApi.Json.feed
        |> Seq.toArray

    let maybeFeedId (page: Read.Page) =
        match page with
        | Read.Recent maybeFeedId -> maybeFeedId
        | Read.Bookmarks -> None


module private View =
    let render flags ctx =
        let flagsJson = Api.serializeObject flags ctx
        let model = Read.initModel flags
        let js = sprintf "SoManyFeeds.StartReadApp(%s);" flagsJson

        Layout.hydrateFableApp Read.view model js


let page
    (listFeedsAndArticles: int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user: User)
    (frontendPage: Read.Page): HttpHandler =

    fun next ctx ->
        task {
            let maybeFeedId = Json.maybeFeedId frontendPage

            match! listFeedsAndArticles maybeFeedId with
            | Ok (feeds, articles) ->
                let (page, selectedFeedId) = Read.pageToFlag frontendPage

                let flags: Read.Flags =
                    { userName = user.Name
                      recents = Json.articleArray feeds articles
                      feeds = Json.feedArray feeds
                      page = page
                      selectedFeedId = selectedFeedId }

                return! htmlView (View.render flags ctx) next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
