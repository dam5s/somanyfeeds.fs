module SoManyFeedsServer.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open SoManyFeedsServer
open SoManyFeedsServer.UserArticlesDataGateway
open SoManyFeedsServer.Json


let private maxFeeds : int =
    Env.varDefaultParse int "MAX_FEEDS" (always "20")


let private authenticatedPage (user : Authentication.User) : WebPart =

    let listFeeds = FeedsDataGateway.listFeeds user.Id
    let createFeed = FeedsService.createFeed maxFeeds user.Id
    let updateFeed = FeedsDataGateway.updateFeed user.Id
    let deleteFeed = FeedsDataGateway.deleteFeed user.Id


    let readPage _ =
        ReadPage.page UserArticlesService.listRecent user None

    let readFeedPage feedId =
        ReadPage.page UserArticlesService.listRecent user (Some feedId)

    let managePage _ =
        ManagePage.page maxFeeds listFeeds user

    let listFeedsApi _ =
        FeedsApi.list listFeeds

    let createFeedApi =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.create createFeed)

    let updateFeedApi feedId =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (updateFeed feedId |> FeedsApi.update)

    let deleteFeedApi feedId =
        FeedsApi.delete
            (fun _ -> deleteFeed feedId)

    let paramToId param =
        unsafeOperation "Reading id query param" { return fun _ -> int64 param }

    let listRecentArticlesApi (request : HttpRequest) =
        let maybeFeedId = request.queryParam "feedId"
                          |> Result.ofChoice
                          |> Result.bind paramToId
                          |> Result.toOption

        ArticlesApi.list (UserArticlesService.listRecent user maybeFeedId)


    let createReadArticle articleId =
        { UserId = user.Id ; ArticleId = articleId }
        |> UserArticlesDataGateway.createReadArticle
        |> ArticlesApi.update

    let deleteReadArticle articleId =
        { UserId = user.Id ; ArticleId = articleId }
        |> UserArticlesDataGateway.deleteReadArticle
        |> ArticlesApi.update

    let createBookmark articleId =
        { UserId = user.Id ; ArticleId = articleId }
        |> UserArticlesDataGateway.createBookmark
        |> ArticlesApi.update

    let deleteBookmark articleId =
        { UserId = user.Id ; ArticleId = articleId }
        |> UserArticlesDataGateway.deleteBookmark
        |> ArticlesApi.update


    choose [
        GET >=> path "/read" >=> request readPage
        GET >=> pathScan "/read/feed/%d" readFeedPage
        GET >=> path "/manage" >=> request managePage

        GET >=> path "/api/feeds" >=> request listFeedsApi
        POST >=> path "/api/feeds" >=> createFeedApi
        PUT >=> pathScan "/api/feeds/%d" updateFeedApi
        DELETE >=> pathScan "/api/feeds/%d" deleteFeedApi

        GET >=> path "/api/articles/recent" >=> request listRecentArticlesApi
        POST >=> pathScan "/api/articles/%d/read" createReadArticle
        DELETE >=> pathScan "/api/articles/%d/read" deleteReadArticle
        POST >=> pathScan "/api/articles/%d/bookmark" createBookmark
        DELETE >=> pathScan "/api/articles/%d/bookmark" deleteBookmark

        NOT_FOUND "not found"
    ]


let webPart =
    let findByEmail = UsersDataGateway.findByEmail
    let createUser = deserializeBody
                         UsersApi.Decoders.registration
                         (UsersApi.create UsersService.create)
    let homePage = DotLiquid.page "home.html.liquid" ()

    choose [
        GET >=> path "/" >=> homePage
        GET >=> Files.browseHome
        GET >=> path "/register" >=> request Authentication.registrationPage
        POST >=> path "/api/users" >=> createUser
        GET >=> path "/login" >=> request Authentication.loginPage
        POST >=> path "/login" >=> request (Authentication.doLogin findByEmail)
        GET >=> path "/logout" >=> request Authentication.doLogout

        request (Authentication.authenticate authenticatedPage)

        UNAUTHORIZED "unauthorized"
    ]
