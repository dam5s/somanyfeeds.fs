module SoManyFeedsServer.Authentication

open System
open Suave
open Suave.Cookie
open Suave.Operators
open Suave.Redirection
open Suave.DotLiquid
open Suave.Response
open Suave.State.CookieStateStore
open SoManyFeedsServer.DataSource
open SoManyFeedsServer.UsersPersistence


type User =
    { Id : int64
      Name : string
    }


module private User =

    open Option.Operators

    let private cookieStoreError : WebPart =
        response HTTP_500 (UTF8.bytes "Cookie store error")

    let private constructor id name =
        { Id = id ; Name = name }


    let tryGet (ctx: HttpContext) : User option =
        ctx
        |> HttpContext.state
        |> Option.bind (fun store ->
            constructor
            <!> store.get "userId"
            <*> store.get "userName"
        )


    let set (user: User) : WebPart =
        context
            (fun ctx ->
                match HttpContext.state ctx with
                | Some store -> store.set "userId" user.Id >=> store.set "userName" user.Name
                | None -> cookieStoreError
            )


let private usingSession : WebPart =
    stateful (CookieLife.MaxAge (TimeSpan.FromDays 2.0)) true

type LoginViewModel =
    { Error : bool }

let private loginError : WebPart =
    page "login.html.liquid" { Error = true }

let loginPage (request : HttpRequest) : WebPart =
    page "login.html.liquid" { Error = false }


let doLogin (findByEmail : string -> FindResult<UserRecord>) (request : HttpRequest) : WebPart =
    let formData name = name
                        |> request.formData
                        |> Choice.orDefault (fun _ -> "")

    match findByEmail (formData "email") with
    | NotFound -> loginError
    | FindError msg -> ErrorPage.page "There was a database access error, please try again later."
    | Found user ->
        if Passwords.verify (formData "password") user.PasswordHash
        then
            usingSession
            >=> User.set { Id = user.Id ; Name = user.Name }
            >=> redirect "/read"
        else
            loginError


let doLogout (request : HttpRequest) : WebPart =
    usingSession
    >=> unsetCookie StateCookie
    >=> redirect "/"


let authenticate (f : User -> WebPart) (request : HttpRequest) : WebPart =
    usingSession
    >=> context
        (fun ctx ->
            match User.tryGet ctx with
            | Some user -> f user
            | None -> redirect "/login"
        )
