[<RequireQualifiedAccess>]
module SoManyFeedsServer.ManageBackend

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsDomain.User
open SoManyFeedsFrontend.Applications.ManageFrontend
open SoManyFeedsServer


let page maxFeeds (listFeeds: AsyncResult<FeedRecord seq>) (user: User) frontendPage =
    fun next ctx ->
        task {
            match! listFeeds with
            | Ok records ->
                let (page, searchText) =
                    match frontendPage with
                    | List -> ("List", None)
                    | Search text -> ("Search", text)

                let flags: Flags =
                    { userName = user.Name
                      maxFeeds = maxFeeds
                      feeds = records
                              |> Seq.map FeedsApi.Json.feed
                              |> Seq.toArray
                      page = page
                      searchText = searchText }

                let flagsJson = Api.serializeObject flags ctx
                let model = initModel flags
                let js = sprintf "SoManyFeeds.StartManageApp(%s);" flagsJson
                let view = Layout.hydrateFableApp view model js

                return! view next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
