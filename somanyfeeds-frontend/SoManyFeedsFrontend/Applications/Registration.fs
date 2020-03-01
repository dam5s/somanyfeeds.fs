module SoManyFeedsFrontend.Applications.Registration

open Elmish
open Fable.React
open Fable.React.Props
open Fable.Import
open Fable.SimpleHttp
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Components.Logo
open SoManyFeedsFrontend.Components.RegistrationForm
open SoManyFeedsFrontend.Support


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
        model, Effects.redirectTo "/read"

let view model d =
  let dispatch = Html.Dispatcher(d)
  let serverErrorView =
      match RegistrationForm.serverError model.Form with
      | "" -> div [] []
      | message -> p [ Class "error message" ] [ str message ]

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
                  [ form [ Class "card"; Method "post"; dispatch.OnSubmit Register ]
                        [ serverErrorView
                          label []
                              [ str "Name"
                                input
                                    [ Placeholder "John"
                                      Name "name"
                                      DefaultValue (RegistrationForm.name model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.updateName)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validateName)
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
                                      DefaultValue (RegistrationForm.email model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.updateEmail)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validateEmail)
                                      Type "email"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.emailError model.Form) ]
                              ]
                          label []
                              [ str "Password"
                                input
                                    [ Placeholder "******************"
                                      Name "password"
                                      DefaultValue (RegistrationForm.password model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.updatePassword)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validatePassword)
                                      Type "password"
                                    ]
                                p [ Class "field-error" ] [ str (RegistrationForm.passwordError model.Form) ]
                              ]
                          label []
                              [ str "Password confirmation"
                                input
                                    [ Placeholder "******************"
                                      Name "passwordConfirmation"
                                      DefaultValue (RegistrationForm.passwordConfirmation model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.updatePasswordConfirmation)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validatePasswordConfirmation)
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
