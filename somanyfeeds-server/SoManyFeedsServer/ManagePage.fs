module SoManyFeedsServer.ManagePage

open SoManyFeeds.FeedsDataGateway
open SoManyFeeds.User
open SoManyFeedsServer
open Suave
open Suave.DotLiquid


type FrontendPage =
    | List
    | Search of string option


type private Flags =
    { UserName: string
      MaxFeeds: int
      Feeds: FeedRecord seq
      Page: FrontendPage
    }


type ManageViewModel =
    { Flags: string
    }


module private Encoders =
    open Chiron
    open Chiron.Operators

    let private feedsEncoder feeds =
        feeds
        |> Seq.map (Json.serializeWith FeedsApi.Encoders.feed)
        |> Seq.toList
        |> Json.Array

    let private pageJson page =
        match page with
        | List -> "List"
        | Search _ -> "Search"

    let private searchText page =
        match page with
        | List -> None
        | Search textOption -> textOption

    let flags flags =
        Json.write "userName" flags.UserName
        *> Json.write "maxFeeds" flags.MaxFeeds
        *> Json.writeWith feedsEncoder "feeds" flags.Feeds
        *> Json.write "page" (pageJson flags.Page)
        *> Json.write "searchText" (searchText flags.Page)


let page maxFeeds (listFeeds: AsyncResult<FeedRecord seq>) (user: User) frontendPage =
    fun ctx -> async {
        match! listFeeds with
        | Ok records ->
            let flags =
                { UserName = user.Name
                  MaxFeeds = maxFeeds
                  Feeds = records
                  Page = frontendPage
                }

            let viewModel =
                { Flags = Json.serializeObject Encoders.flags flags }

            return! page "manage.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
