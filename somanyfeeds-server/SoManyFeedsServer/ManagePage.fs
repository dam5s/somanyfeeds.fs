module SoManyFeedsServer.ManagePage

open Suave
open Suave.DotLiquid
open SoManyFeedsServer
open SoManyFeedsServer.FeedsPersistence


type FeedViewModel =
    { Id : int64
      Name : string
    }


type ManageViewModel =
    { UserName : string
      Feeds : FeedViewModel list
    }


let private presentFeed (record : FeedRecord) : FeedViewModel =
    { Id = record.Id
      Name = record.Name
    }


let page (listFeeds : int64 -> Result<FeedRecord list, string>) (user : Authentication.User) : WebPart =
    match listFeeds user.Id with
    | Ok records ->
        let viewModel =
            { UserName = user.Name
              Feeds = List.map presentFeed records
            }
        page "manage.html.liquid" viewModel
    | Error message ->
        ErrorPage.page message
