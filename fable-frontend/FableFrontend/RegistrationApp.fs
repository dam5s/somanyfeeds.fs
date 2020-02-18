module FableFrontend.RegistrationApp

open Elmish
open Fable.React
open Fable.React.Props
open Fable.Import
open Fable.SimpleHttp
open FableFrontend.HttpSupport
open FableFrontend.RegistrationForm


type Model =
    { Form: RegistrationForm }

type Msg =
    | Register
    | UpdateForm of (RegistrationForm -> string -> RegistrationForm) * string
    | ValidateField of (RegistrationForm -> RegistrationForm)
    | RegistrationResult of Result<unit, RequestError>

let init () =
    { Form = RegistrationForm.create }, Cmd.none

let private sendRequest form =
    async {
        let! response =
            form
            |> RegistrationForm.request
            |> Http.send

        return
            if response.statusCode = 201
                then Ok ()
                else Error ApiError
    }

let private redirectTo destination =
    Browser.Dom.window.location.assign destination
    Cmd.none

let update msg model =
    match msg with
    | Register ->
        match RegistrationForm.validate model.Form with
        | Ok validForm ->
            ( model, Cmd.ofRequest sendRequest validForm RegistrationResult )
        | Error formWithErrors ->
            { model with Form = formWithErrors }, Cmd.none

    | UpdateForm (updateFunction, newValue) ->
        { model with Form = updateFunction model.Form newValue }, Cmd.none

    | ValidateField validationFunction ->
        { model with Form = validationFunction model.Form }, Cmd.none

    | RegistrationResult (Error err) ->
        { model with Form = RegistrationForm.applyErrors err model.Form }, Cmd.none

    | RegistrationResult (Ok _) ->
        model, redirectTo "/read"

let view model dispatch =
  let serverErrorView =
      match RegistrationForm.serverError model.Form with
      | "" -> div [] []
      | message -> p [ Class "error message" ] [ str message ]

  let onSubmit msg = OnSubmit (fun event -> event.preventDefault(); dispatch msg)
  let onInput msg = OnInput (fun event -> dispatch (msg event.Value))
  let onBlur msg = OnBlur (fun _ -> dispatch msg)

  div []
      [ header [ Class "app-header" ]
            [ div []
                  [ Logo.view
                    nav [] []
                  ]
            ]
        header [ Class "page" ]
            [ div [ Class "page-content" ]
                  [ h2 [] [ str "Home" ]
                    h1 [] [ str "Registration" ]
                  ]
            ]
        div [ Class "main" ]
            [ section []
                  [ form [ Class "card"; Method "post"; onSubmit Register ]
                        [ serverErrorView
                          label []
                              [ str "Name"
                                input
                                    [ Placeholder "John"
                                      Name "name"
                                      Value (RegistrationForm.name model.Form)
                                      onInput (curry UpdateForm RegistrationForm.updateName)
                                      onBlur (ValidateField RegistrationForm.validateName)
                                      AutoFocus true
                                      Type "text"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.nameError model.Form) ]
                              ]
                          label []
                              [ str "Email"
                                input
                                    [ Placeholder "john@example.com"
                                      Name "email"
                                      Value (RegistrationForm.email model.Form)
                                      onInput (curry UpdateForm RegistrationForm.updateEmail)
                                      onBlur (ValidateField RegistrationForm.validateEmail)
                                      Type "email"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.emailError model.Form) ]
                              ]
                          label []
                              [ str "Password"
                                input
                                    [ Placeholder "******************"
                                      Name "password"
                                      Value (RegistrationForm.password model.Form)
                                      onInput (curry UpdateForm RegistrationForm.updatePassword)
                                      onBlur (ValidateField RegistrationForm.validatePassword)
                                      Type "password"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.passwordError model.Form) ]
                              ]
                          label []
                              [ str "Password confirmation"
                                input
                                    [ Placeholder "******************"
                                      Name "passwordConfirmation"
                                      Value (RegistrationForm.passwordConfirmation model.Form)
                                      onInput (curry UpdateForm RegistrationForm.updatePasswordConfirmation)
                                      onBlur (ValidateField RegistrationForm.validatePasswordConfirmation)
                                      Type "password"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.passwordConfirmationError model.Form) ]
                              ]
                          nav []
                              [ button [ Class "button primary" ] [ str "Sign up" ]
                              ]
                        ]
                  ]
            ]
      ]
