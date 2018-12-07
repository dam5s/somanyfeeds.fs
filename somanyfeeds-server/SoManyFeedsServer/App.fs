module SoManyFeedsServer.App

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Redirection
open Suave.DotLiquid
open SoManyFeedsServer


type ReadViewModel =
    { userName : string }


type ManageViewModel =
    { userName : string }


let private readPage (user : Authentication.User) : WebPart =
    page "read.html.liquid" { userName = user.name }


let private managePage (user : Authentication.User) : WebPart =
    page "manage.html.liquid" { userName = user.name }


let private authenticatedPage (user : Authentication.User) : WebPart =
    choose [
        GET >=> path "/" >=> redirect "/read"
        GET >=> path "/read" >=> readPage user
        GET >=> path "/manage" >=> managePage user

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]


let handler =
    choose [
        Authentication.authenticate authenticatedPage
        UNAUTHORIZED "unauthorized"
    ]
