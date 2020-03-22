module SoManyFeedsFrontend.Applications.Registration

open Elmish
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Components.Logo
open SoManyFeedsFrontend.Components.RegistrationForm
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Form


type Model =
    { Form: RegistrationForm }

type Msg =
    | Register
    | UpdateForm of (string -> RegistrationForm -> RegistrationForm) * string
    | ValidateField of (RegistrationForm -> RegistrationForm)
    | RegistrationResult of Result<unit, RequestError>

let initModel =
    { Form = RegistrationForm.create }

let init () =
    initModel, Cmd.none

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
        { model with Form = updateFunction newValue model.Form }, Cmd.none

    | ValidateField validationFunction ->
        { model with Form = validationFunction model.Form }, Cmd.none

    | RegistrationResult (Error err) ->
        { model with Form = Form.applyRequestError err model.Form }, Cmd.none

    | RegistrationResult (Ok _) ->
        model, Effects.redirectTo "/read"

let view model d =
  let dispatch = Html.Dispatcher(d)
  let serverErrorView =
      match Form.serverError model.Form with
      | None -> div [] []
      | Some message -> p [ Class "error message" ] [ str message ]

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
                                      DefaultValue (RegistrationForm.name.get model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.name.set)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validateName)
                                      AutoFocus true
                                      Type "text"
                                    ]
                                p [ Class "field-error" ] [ str (Form.fieldError "name" model.Form) ]
                              ]
                          label []
                              [ str "Email"
                                input
                                    [ Placeholder "john@example.com"
                                      Name "email"
                                      DefaultValue (RegistrationForm.email.get model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.email.set)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validateEmail)
                                      Type "email"
                                    ]
                                p [ Class "field-error" ] [ str (Form.fieldError "email" model.Form) ]
                              ]
                          label []
                              [ str "Password"
                                input
                                    [ Placeholder "******************"
                                      Name "password"
                                      DefaultValue (RegistrationForm.password.get model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.password.set)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validatePassword)
                                      Type "password"
                                    ]
                                p [ Class "field-error" ] [ str (Form.fieldError "password" model.Form) ]
                              ]
                          label []
                              [ str "Password confirmation"
                                input
                                    [ Placeholder "******************"
                                      Name "passwordConfirmation"
                                      DefaultValue (RegistrationForm.passwordConfirmation.get model.Form)
                                      dispatch.OnChange (curry UpdateForm RegistrationForm.passwordConfirmation.set)
                                      dispatch.OnBlur (ValidateField RegistrationForm.validatePasswordConfirmation)
                                      Type "password"
                                    ]
                                p [ Class "field-error" ] [ str (Form.fieldError "passwordConfirmation" model.Form) ]
                              ]
                          nav []
                              [ button [ Class "button primary" ] [ str "Sign up" ]
                              ]
                        ]
                  ]
            ]
      ]
