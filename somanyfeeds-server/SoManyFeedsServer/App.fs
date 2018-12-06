module SoManyFeedsServer.App

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open SoManyFeedsServer


let private articlesPage (user : Authentication.User) : WebPart =
    Successful.OK <| String.Format ("Hello {0}, read your articles here.", user.name)


let private feedsPage (user : Authentication.User) : WebPart =
    Successful.OK <| String.Format ("Hello {0}, manage your feeds here.", user.name)


let private authenticatedPage (user : Authentication.User) : WebPart =
    choose [
        path "/feeds" >=> GET >=> feedsPage user
        path "/" >=> GET >=> articlesPage user

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]


let handler =
    choose [
        Authentication.authenticate authenticatedPage
        UNAUTHORIZED "unauthorized"
    ]
