[<RequireQualifiedAccess>]
module SoManyFeedsServer.ManageBackend

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http

open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsDomain.User
open SoManyFeedsFrontend.Applications
open SoManyFeedsFrontend.Applications.ManageFrontend
open SoManyFeedsServer

let private queryParamValues name (context: HttpContext) =
    context.Request.Query.Item(name).ToArray()

let page maxFeeds (listFeeds: AsyncResult<FeedRecord seq>) (user: User) (frontendPage: ManageFrontend.Page) =
    fun next ctx ->
        task {
            let q =
                ctx 
                |> queryParamValues "q"
                |> Array.tryHead 
                |> Option.defaultValue ""

            match! listFeeds with
            | Ok records ->
                let flags: Flags =
                    { UserName = user.Name
                      MaxFeeds = maxFeeds
                      Feeds = records
                              |> Seq.map FeedsApi.Json.feed
                              |> Seq.toArray
                      Page = sprintf "%A" frontendPage
                      SearchText = q }

                let flagsJson = Api.serializeObject flags ctx
                let js = sprintf "SoManyFeeds.StartManageApp(%s);" flagsJson
                let model = initModel flags
                let page = Layout.hydrateFableApp view model js

                return! page next ctx
            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
