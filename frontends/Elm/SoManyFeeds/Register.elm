module SoManyFeeds.Register exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, button, div, form, h1, h2, header, input, label, nav, p, section, text)
import Html.Attributes exposing (autofocus, class, href, placeholder, type_, value)
import Html.Events exposing (onBlur, onInput, onSubmit)
import Http
import SoManyFeeds.Logo as Logo
import SoManyFeeds.RedirectTo exposing (redirectTo)
import SoManyFeeds.RegistrationForm as RegistrationForm exposing (RegistrationForm, ValidRegistrationForm)


type alias Flags =
    {}


type alias Model =
    { form : RegistrationForm
    }


type Msg
    = Register
    | UpdateForm (RegistrationForm -> String -> RegistrationForm) String
    | ValidateField (RegistrationForm -> RegistrationForm)
    | RegistrationResult (Result Http.Error String)


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { form = RegistrationForm.new }
    , Cmd.none
    )


view : Model -> Document Msg
view model =
    let
        serverErrorView =
            case RegistrationForm.serverError model.form of
                "" ->
                    div [] []

                message ->
                    p [ class "error message" ] [ text message ]
    in
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [ class "app-header" ]
            [ div []
                [ Logo.view
                , nav []
                    [ a [ href "/", class "current" ] [ text "Home" ]
                    , a [ href "/read" ] [ text "Read" ]
                    , a [ href "/manage" ] [ text "Manage" ]
                    ]
                ]
            ]
        , header [ class "page" ]
            [ h2 [] [ text "Home" ]
            , h1 [] [ text "Registration" ]
            ]
        , div [ class "main" ]
            [ section []
                [ form [ class "card", onSubmit Register ]
                    [ serverErrorView
                    , label []
                        [ text "Name"
                        , input
                            [ placeholder "John"
                            , value <| RegistrationForm.name model.form
                            , onInput <| UpdateForm RegistrationForm.updateName
                            , onBlur <| ValidateField RegistrationForm.validateName
                            , autofocus True
                            , type_ "text"
                            ]
                            []
                        , p [ class "field-error" ] [ text <| RegistrationForm.nameError model.form ]
                        ]
                    , label []
                        [ text "Email"
                        , input
                            [ placeholder "john@example.com"
                            , value <| RegistrationForm.email model.form
                            , onInput <| UpdateForm RegistrationForm.updateEmail
                            , onBlur <| ValidateField RegistrationForm.validateEmail
                            , type_ "text"
                            ]
                            []
                        , p [ class "field-error" ] [ text <| RegistrationForm.emailError model.form ]
                        ]
                    , label []
                        [ text "Password"
                        , input
                            [ placeholder "******************"
                            , value <| RegistrationForm.password model.form
                            , onInput <| UpdateForm RegistrationForm.updatePassword
                            , onBlur <| ValidateField RegistrationForm.validatePassword
                            , type_ "password"
                            ]
                            []
                        , p [ class "field-error" ] [ text <| RegistrationForm.passwordError model.form ]
                        ]
                    , label []
                        [ text "Password confirmation"
                        , input
                            [ placeholder "******************"
                            , value <| RegistrationForm.passwordConfirmation model.form
                            , onInput <| UpdateForm RegistrationForm.updatePasswordConfirmation
                            , onBlur <| ValidateField RegistrationForm.validatePasswordConfirmation
                            , type_ "password"
                            ]
                            []
                        , p [ class "field-error" ] [ text <| RegistrationForm.passwordConfirmationError model.form ]
                        ]
                    , nav []
                        [ button [ class "button primary" ] [ text "Sign up" ]
                        ]
                    ]
                ]
            ]
        ]
    }


sendRequest : ValidRegistrationForm -> Cmd Msg
sendRequest validForm =
    Http.send RegistrationResult (RegistrationForm.request validForm)


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        Register ->
            case RegistrationForm.validate model.form of
                Ok validForm ->
                    ( model, sendRequest validForm )

                Err formWithErrors ->
                    ( { model | form = formWithErrors }, Cmd.none )

        UpdateForm updateFunction newValue ->
            ( { model | form = updateFunction model.form newValue }, Cmd.none )

        ValidateField validationFunction ->
            ( { model | form = validationFunction model.form }, Cmd.none )

        RegistrationResult (Err error) ->
            ( { model | form = RegistrationForm.applyHttpError error model.form }, Cmd.none )

        RegistrationResult (Ok _) ->
            ( model, redirectTo "/read" )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
