[<RequireQualifiedAccess>]
module SoManyFeedsServer.ManagePage

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeeds.FeedsDataGateway
open SoManyFeeds.User
open SoManyFeedsServer


type FrontendPage =
    | List
    | Search of string option


type private Flags =
    { UserName: string
      MaxFeeds: int
      Feeds: FeedRecord seq
      Page: FrontendPage }


module private Json =
    let private pageJson page =
        match page with
        | List -> "List"
        | Search _ -> "Search"

    let private searchText page =
        match page with
        | List -> None
        | Search textOption -> textOption

    let flags flags =
        {| userName = flags.UserName
           maxFeeds = flags.MaxFeeds
           feeds =
               flags.Feeds
               |> Seq.map FeedsApi.Json.feed
               |> Seq.toList
           page = pageJson flags.Page
           searchText = searchText flags.Page |}


module private View =
    let render (flagsJson: string) =
        flagsJson
        |> sprintf "SoManyFeeds.StartManageApp(%s);"
        |> Layout.startFableApp


let page maxFeeds (listFeeds: AsyncResult<FeedRecord seq>) (user: User) frontendPage =
    fun next ctx ->
        task {
            match! listFeeds with
            | Ok records ->
                let flags =
                    { UserName = user.Name
                      MaxFeeds = maxFeeds
                      Feeds = records
                      Page = frontendPage }

                let flagsJson = Api.serializeObject (Json.flags flags) ctx

                return! htmlView (View.render flagsJson) next ctx
            | Error message ->
                return! ErrorPage.page message next ctx
        }
