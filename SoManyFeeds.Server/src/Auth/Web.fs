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
    open Fable.React
    open Fable.React.Props

    let login (error: bool) =
        let errorView =
            if error
            then p [ Class "error message" ] [ str "Authentication failed." ]
            else div [] []

        [ header [ Class "page" ]
              [ div [ Class "page-content" ]
                    [ h2 [] [ str "Login" ]
                      h1 [] [ str "Authentication required" ]
                    ]
              ]
          div [ Class "main" ]
              [ section []
                    [ form [ Method "post"; Class "card" ]
                          [ errorView
                            label []
                                [ str "Email"
                                  input [ Type "email"; Name "email"; Placeholder "john@example.com" ]
                                ]
                            label []
                                [ str "Password"
                                  input [ Type "password"; Name "password"; Placeholder "******************" ]
                                ]
                            nav [] [ button [ Class "button primary" ] [ str "Sign in" ] ]
                          ]
                    ]
                section []
                    [ div [ Class "card" ]
                          [ p [ Class "message" ]
                                [ str "Don't have an account? "
                                  a [ Href "/register" ] [ str "Sign up now" ]
                                  str "."
                                ]
                          ]
                    ]
              ]
        ]

let loginPage error =
    Layout.withoutTabs (Views.login error)

let private loginError =
    loginPage true

let private formData name (ctx: HttpContext) =
    name
    |> ctx.GetFormValue
    |> Option.defaultValue ""

let doLogin (loginByEmailAndPassword: string -> string -> Async<FindResult<UserRecord>>): HttpHandler =
    fun next ctx ->
        task {
            let email = formData "email" ctx
            let password = formData "password" ctx

            match! loginByEmailAndPassword email password with
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
    Layout.hydrateFableApp
        RegistrationFrontend.view
        RegistrationFrontend.initModel
        "SoManyFeeds.StartRegistrationApp();"

let authenticate (withUser: User -> HttpHandler): HttpHandler =
    fun next ctx ->
        match Session.getUser ctx with
        | Some user -> 
            withUser user next ctx
        | None ->
            ctx
            |> Session.saveUrl
            |> redirectTo false "/login" next
