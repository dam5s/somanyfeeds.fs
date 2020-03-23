module SoManyFeedsServer.Auth.Web

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open SoManyFeedsPersistence.DataSource
open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsDomain.User
open SoManyFeedsFrontend.Applications
open SoManyFeedsServer
open SoManyFeedsServer.Auth


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

let doLogin (findByEmailAndPassword: string -> string -> Async<FindResult<UserRecord>>): HttpHandler =
    fun next ctx ->
        task {
            let email = formData "email" ctx
            let password = formData "password" ctx

            match! findByEmailAndPassword email password with
            | NotFound ->
                return! loginError next ctx
            | FindError e ->
                let explanation = { e with Message = "There was a database access error, please try again later." }
                return! ErrorPage.page explanation next ctx
            | Found userRecord ->
                let user =
                    { Id = userRecord.Id
                      Name = userRecord.Name }
                return!
                    ctx
                    |> Session.setUser user
                    |> Session.takeUrl
                    |> Option.defaultValue "/"
                    |> fun url -> redirectTo false url next ctx
        }

let doLogout: HttpHandler =
    fun next ctx ->
        ctx
        |> Session.clear
        |> redirectTo false "/" next

let registrationPage: HttpHandler =
    htmlView (
        Layout.hydrateFableApp
            RegistrationFrontend.view
            RegistrationFrontend.initModel
            "SoManyFeeds.StartRegistrationApp();"
    )

let authenticate (withUser: User -> HttpHandler): HttpHandler =
    fun next ctx ->
        match Session.getUser ctx with
        | Some user -> withUser user next ctx
        | None ->
            ctx
            |> Session.saveUrl
            |> redirectTo false "/login" next
