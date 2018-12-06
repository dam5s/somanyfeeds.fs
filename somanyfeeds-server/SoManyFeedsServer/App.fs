module SoManyFeedsServer.App

open System
open Suave
open SoManyFeedsServer


let homePage (user : Authentication.User) : WebPart =
    Successful.OK <| String.Format("hello {0}", user.name)


let handler =
    Authentication.authenticate homePage
