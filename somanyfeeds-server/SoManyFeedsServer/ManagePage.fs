module SoManyFeedsServer.ManagePage

open Suave
open Suave.DotLiquid
open SoManyFeedsServer
open SoManyFeedsServer.FeedsDataGateway


type ManageViewModel =
    { UserName : string
      MaxFeeds : int
      FeedsJson : string
    }


let page (maxFeeds : int) (listFeeds : AsyncResult<FeedRecord seq>) (user : Authentication.User) : WebPart =
    fun ctx -> async {
        let! listResult = listFeeds

        match listResult with
        | Ok records ->
            let viewModel =
                { UserName = user.Name
                  MaxFeeds = maxFeeds
                  FeedsJson = Json.serializeList FeedsApi.Encoders.feed records
                }
            return! page "manage.html.liquid" viewModel ctx
        | Error message ->
            return! ErrorPage.page message ctx
    }
