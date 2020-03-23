[<RequireQualifiedAccess>]
module SoManyFeedsServer.ManagePage

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsDomain.User
open SoManyFeedsFrontend.Applications
open SoManyFeedsServer


let private pageFlags (page: Manage.Page) =
    match page with
    | Manage.List -> ("List", None)
    | Manage.Search text -> ("Search", text)


module private View =
    let render (flags: Manage.Flags) ctx =
        let flagsJson = Api.serializeObject flags ctx
        let model = Manage.initModel flags
        let js = sprintf "SoManyFeeds.StartManageApp(%s);" flagsJson

        Layout.hydrateFableApp Manage.view model js


let page maxFeeds (listFeeds: AsyncResult<FeedRecord seq>) (user: User) frontendPage =
    fun next ctx ->
        task {
            match! listFeeds with
            | Ok records ->
                let (page, searchText) = pageFlags frontendPage
                let flags: Manage.Flags =
                    { userName = user.Name
                      maxFeeds = maxFeeds
                      feeds = records
                              |> Seq.map FeedsApi.Json.feed
                              |> Seq.toArray
                      page = page
                      searchText = searchText }

                let view = View.render flags ctx

                return! htmlView view next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
