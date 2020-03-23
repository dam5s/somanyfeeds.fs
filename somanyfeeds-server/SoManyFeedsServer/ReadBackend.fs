[<RequireQualifiedAccess>]
module SoManyFeedsServer.ReadPage

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsFrontend.Applications.ReadFrontend
open SoManyFeedsDomain.User


let page
    (listFeedsAndArticles: int64 option -> AsyncResult<FeedRecord seq * ArticleRecord seq>)
    (user: User)
    (frontendPage: Page): HttpHandler =

    fun next ctx ->
        task {
            let maybeFeedId =
                match frontendPage with
                | Recent maybeFeedId -> maybeFeedId
                | Bookmarks -> None

            match! listFeedsAndArticles maybeFeedId with
            | Ok (feeds, articles) ->
                let (page, selectedFeedId) =
                    match frontendPage with
                    | Recent feedId -> ("Recent", feedId)
                    | Bookmarks -> ("Bookmarks", None)

                let flags: Flags =
                    { userName = user.Name
                      recents = articles
                                |> Seq.map (ArticlesApi.Json.article feeds)
                                |> Seq.toArray
                      feeds = feeds
                              |> Seq.map FeedsApi.Json.feed
                              |> Seq.toArray
                      page = page
                      selectedFeedId = selectedFeedId }

                let flagsJson = Api.serializeObject flags ctx
                let model = initModel flags
                let js = sprintf "SoManyFeeds.StartReadApp(%s);" flagsJson
                let view = Layout.hydrateFableApp view model js

                return! htmlView view next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
