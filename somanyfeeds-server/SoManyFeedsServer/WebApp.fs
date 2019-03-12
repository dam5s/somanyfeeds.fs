module SoManyFeedsServer.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open SoManyFeedsServer
open SoManyFeedsServer.DataSource
open SoManyFeedsServer.Json


let private maxFeeds : int =
    Env.varDefaultParse int "MAX_FEEDS" (always "20")


let private authenticatedPage (user : Authentication.User) : WebPart =

    let listFeeds = FeedsDataGateway.listFeeds dataSource user.Id
    let createFeed = FeedsService.createFeed dataSource maxFeeds user.Id
    let updateFeed = FeedsDataGateway.updateFeed dataSource user.Id
    let deleteFeed = FeedsDataGateway.deleteFeed dataSource user.Id

    let listRecentArticles = UserArticlesService.listRecent dataSource user
    let createReadArticle articleId =
        UserArticlesDataGateway.createReadArticle
            dataSource
            { UserId = user.Id ; ArticleId = articleId }
    let deleteReadArticle articleId =
        UserArticlesDataGateway.deleteReadArticle
            dataSource
            { UserId = user.Id ; ArticleId = articleId }


    let readPage _ =
        ReadPage.page listRecentArticles user

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
            (FeedsApi.update <| updateFeed feedId)

    let deleteFeedApi feedId =
        FeedsApi.delete
            (fun _ -> deleteFeed feedId)

    let listRecentArticlesApi _ =
        ArticlesApi.list listRecentArticles

    let createReadArticleApi articleId =
        ArticlesApi.update (createReadArticle articleId)

    let deleteReadArticleApi articleId =
        ArticlesApi.update (deleteReadArticle articleId)


    choose [
        GET >=> path "/read" >=> request readPage
        GET >=> path "/manage" >=> request managePage

        GET >=> path "/api/feeds" >=> request listFeedsApi
        POST >=> path "/api/feeds" >=> createFeedApi
        PUT >=> pathScan "/api/feeds/%d" updateFeedApi
        DELETE >=> pathScan "/api/feeds/%d" deleteFeedApi

        GET >=> path "/api/articles/recent" >=> request listRecentArticlesApi
        POST >=> pathScan "/api/articles/%d/read" createReadArticleApi
        DELETE >=> pathScan "/api/articles/%d/read" deleteReadArticleApi

        NOT_FOUND "not found"
    ]


let webPart =
    let findByEmail = UsersDataGateway.findByEmail DataSource.dataContext
    let createUser = deserializeBody
                         UsersApi.Decoders.registration
                         (UsersApi.create <| UsersService.create DataSource.dataContext)
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
