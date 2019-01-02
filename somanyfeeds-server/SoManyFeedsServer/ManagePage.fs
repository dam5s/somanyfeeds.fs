module SoManyFeedsServer.ManagePage

open Suave
open Suave.DotLiquid
open SoManyFeedsServer
open SoManyFeedsServer.FeedsPersistence


type ManageViewModel =
    { UserName : string
      FeedsJson : string
    }


let page (listFeeds : unit -> Result<FeedRecord list, string>) (user : Authentication.User) : WebPart =
    match listFeeds () with
    | Ok records ->
        let viewModel =
            { UserName = user.Name
              FeedsJson = Json.serializeList FeedsApi.Encoders.feed records
            }
        page "manage.html.liquid" viewModel
    | Error message ->
        ErrorPage.page message
