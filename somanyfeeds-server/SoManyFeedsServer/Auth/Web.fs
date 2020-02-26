module SoManyFeedsServer.Auth.Web

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open SoManyFeedsDomain
open SoManyFeedsPersistence.DataSource
open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsDomain.User
open SoManyFeedsServer
open SoManyFeedsServer.Auth
open SoManyFeedsServer.CacheBusting


module private Views =
    open GiraffeViewEngine

    let login (error: bool) =
        let errorView =
            if error
            then p [ _class "error message" ] [ rawText "Authentication failed." ]
            else div [] []

        [ header [ _class "page" ]
              [ div [ _class "page-content" ]
                    [ h2 [] [ rawText "Login" ]
                      h1 [] [ rawText "Authentication required" ]
                    ]
              ]
          div [ _class "main" ]
              [ section []
                    [ form [ _method "post"; _class "card" ]
                          [ errorView
                            label []
                                [ rawText "Email"
                                  input [ _type "email"; _name "email"; _placeholder "john@example.com" ]
                                ]
                            label []
                                [ rawText "Password"
                                  input [ _type "password"; _name "password"; _placeholder "******************" ]
                                ]
                            nav [] [ button [ _class "button primary" ] [ rawText "Sign in" ] ]
                          ]
                    ]
                section []
                    [ div [ _class "card" ]
                          [ p [ _class "message" ]
                                [ rawText "Don't have an account? "
                                  a [ _href "/register" ] [ rawText "Sign up now" ]
                                  rawText "."
                                ]
                          ]
                    ]
              ]
        ]

let loginPage error =
    Layout.withoutTabs (Views.login error)

let private loginError =
    htmlView (loginPage true)

let private formData name (ctx: HttpContext) =
    name
    |> ctx.GetFormValue
    |> Option.defaultValue ""

let doLogin (findByEmail: string -> Async<FindResult<UserRecord>>): HttpHandler =
    fun next ctx ->
        task {
            match! findByEmail (formData "email" ctx) with
            | NotFound ->
                return! loginError next ctx
            | FindError _ ->
                return! ErrorPage.page "There was a database access error, please try again later." next ctx
            | Found userRecord ->
                let user =
                    { Id = userRecord.Id
                      Name = userRecord.Name }
                return! if Passwords.verify (formData "password" ctx) userRecord.PasswordHash
                        then
                            ctx
                            |> Session.setUser user
                            |> Session.takeUrl
                            |> Option.defaultValue "/"
                            |> fun url -> redirectTo false url next ctx
                        else
                            loginError next ctx
        }

let doLogout: HttpHandler =
    fun next ctx ->
        ctx
        |> Session.clear
        |> redirectTo false "/" next

let registrationPage: HttpHandler =
    htmlView (Layout.startFableApp "SoManyFeeds.StartRegistrationApp();")

let authenticate (withUser: User -> HttpHandler): HttpHandler =
    fun next ctx ->
        match Session.getUser ctx with
        | Some user -> withUser user next ctx
        | None ->
            ctx
            |> Session.saveUrl
            |> redirectTo false "/login" next
