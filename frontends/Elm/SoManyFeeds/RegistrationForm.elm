module SoManyFeeds.RegistrationForm exposing (RegistrationForm, ValidRegistrationForm, email, emailError, isValid, name, nameError, new, password, passwordConfirmation, passwordConfirmationError, passwordError, request, updateEmail, updateName, updatePassword, updatePasswordConfirmation, validate, validateEmail, validateName, validatePassword, validatePasswordConfirmation)

import Http
import Json.Encode


type Error
    = Error String


type alias Field =
    { value : String
    , error : Maybe Error
    }


type RegistrationForm
    = RegistrationForm
        { name : Field
        , email : Field
        , password : Field
        , passwordConfirmation : Field
        }


type ValidRegistrationForm
    = ValidRegistrationForm
        { name : String
        , email : String
        , password : String
        , passwordConfirmation : String
        }


new : RegistrationForm
new =
    let
        newField =
            { value = "", error = Nothing }
    in
    RegistrationForm
        { name = newField
        , email = newField
        , password = newField
        , passwordConfirmation = newField
        }


fields (RegistrationForm form) =
    form


maybeErrorToString : Maybe Error -> String
maybeErrorToString maybeErr =
    case maybeErr of
        Nothing ->
            ""

        Just (Error value) ->
            value


name =
    fields >> .name >> .value


nameError =
    fields >> .name >> .error >> maybeErrorToString


email =
    fields >> .email >> .value


emailError =
    fields >> .email >> .error >> maybeErrorToString


password =
    fields >> .password >> .value


passwordError =
    fields >> .password >> .error >> maybeErrorToString


passwordConfirmation =
    fields >> .passwordConfirmation >> .value


passwordConfirmationError =
    fields >> .passwordConfirmation >> .error >> maybeErrorToString


updateField field newValue =
    { field | value = newValue, error = Nothing }


updateName (RegistrationForm form) newValue =
    RegistrationForm { form | name = updateField form.name newValue }


updateEmail (RegistrationForm form) newValue =
    RegistrationForm { form | email = updateField form.email newValue }


updatePassword (RegistrationForm form) newValue =
    RegistrationForm { form | password = updateField form.password newValue }


updatePasswordConfirmation (RegistrationForm form) newValue =
    RegistrationForm { form | passwordConfirmation = updateField form.passwordConfirmation newValue }


request : ValidRegistrationForm -> Http.Request String
request (ValidRegistrationForm form) =
    let
        body =
            Http.jsonBody <|
                Json.Encode.object
                    [ ( "name", Json.Encode.string form.name )
                    , ( "email", Json.Encode.string form.email )
                    , ( "password", Json.Encode.string form.password )
                    , ( "passwordConfirmation", Json.Encode.string form.passwordConfirmation )
                    ]
    in
    Http.request
        { method = "POST"
        , headers = []
        , url = "/api/users"
        , body = body
        , expect = Http.expectString
        , timeout = Nothing
        , withCredentials = False
        }


validate : RegistrationForm -> Result RegistrationForm ValidRegistrationForm
validate form =
    let
        validatedForm =
            form
                |> validateName
                |> validateEmail
                |> validatePassword
                |> validatePasswordConfirmation

        validForm (RegistrationForm f) =
            ValidRegistrationForm
                { name = f.name.value
                , email = f.email.value
                , password = f.password.value
                , passwordConfirmation = f.passwordConfirmation.value
                }
    in
    if isValid validatedForm then
        Ok (validForm form)

    else
        Err validatedForm


validateName (RegistrationForm form) =
    RegistrationForm { form | name = validateField nameValidation form.name }


validateEmail (RegistrationForm form) =
    RegistrationForm { form | email = validateField emailValidation form.email }


validatePassword (RegistrationForm form) =
    RegistrationForm { form | password = validateField passwordValidation form.password }


validatePasswordConfirmation (RegistrationForm form) =
    RegistrationForm { form | passwordConfirmation = validateField (passwordConfirmationValidation form.password.value) form.passwordConfirmation }


validateField : (String -> Maybe Error) -> Field -> Field
validateField validator field =
    { field | error = validator field.value }


isValid : RegistrationForm -> Bool
isValid (RegistrationForm form) =
    [ form.name
    , form.email
    , form.password
    , form.passwordConfirmation
    ]
        |> List.all fieldIsValid


fieldIsValid : Field -> Bool
fieldIsValid field =
    field.error
        |> Maybe.map (always False)
        |> Maybe.withDefault True


emailValidation : String -> Maybe Error
emailValidation value =
    let
        lengthIsValid =
            String.length value > 2

        isEmail =
            String.contains "@" value
    in
    if lengthIsValid && isEmail then
        Nothing

    else
        Just (Error "email address required")


passwordValidation : String -> Maybe Error
passwordValidation value =
    if String.length value >= 8 then
        Nothing

    else
        Just (Error "at least 8 characters required")


passwordConfirmationValidation : String -> String -> Maybe Error
passwordConfirmationValidation original copy =
    if original == copy then
        Nothing

    else
        Just (Error "confirmation mismatched")


nameValidation : String -> Maybe Error
nameValidation value =
    if String.isEmpty value then
        Just (Error "cannot be blank")

    else
        Nothing
