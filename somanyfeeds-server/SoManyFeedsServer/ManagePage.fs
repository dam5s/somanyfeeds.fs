module SoManyFeedsServer.ManagePage

open Suave
open Suave.DotLiquid
open SoManyFeedsServer
open SoManyFeedsServer.FeedsDataGateway


type FrontendPage =
    | List
    | Search of string option


type private Flags =
    { UserName : string
      MaxFeeds : int
      Feeds : FeedRecord seq
      Page : FrontendPage
    }


type ManageViewModel =
    { Flags : string
    }


module private Encoders =
    open Chiron
    open Chiron.Operators

    let feedsEncoder (feeds : FeedRecord seq) : Json =
        feeds
        |> Seq.map (Json.serializeWith FeedsApi.Encoders.feed)
        |> Seq.toList
        |> Json.Array

    let pageJson (page : FrontendPage) =
        match page with
        | List -> "List"
        | Search _ -> "Search"

    let searchText (page : FrontendPage) =
        match page with
        | List -> None
        | Search textOption -> textOption

    let flags (flags : Flags) : Json<unit> =
        Json.write "userName" flags.UserName
        *> Json.write "maxFeeds" flags.MaxFeeds
        *> Json.writeWith feedsEncoder "feeds" flags.Feeds
        *> Json.write "page" (pageJson flags.Page)
        *> Json.write "searchText" (searchText flags.Page)


let page (maxFeeds : int) (listFeeds : AsyncResult<FeedRecord seq>) (user : Authentication.User) (frontendPage : FrontendPage) : WebPart =
    fun ctx -> async {
        let! listResult = listFeeds

        match listResult with
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
