module FableFrontend.RegistrationForm


type private FormError = FormError of string

[<RequireQualifiedAccess>]
module private FormError =
    let optionToString (err: FormError option) =
        match err with
        | Some (FormError msg) -> msg
        | _ -> ""

type private Field =
    { Value: string
      Error: FormError option
    }

[<RequireQualifiedAccess>]
module private Field =
    let setError newErr (field: Field) = { field with Error = newErr }
    let setValue newValue (field: Field) = { field with Value = newValue }
    let validate validator field = setError (validator field.Value) field
    let isValid field = field.Error |> Option.isNone


type RegistrationForm =
    private { ServerError: FormError option
              Name: Field
              Email: Field
              Password: Field
              PasswordConfirmation: Field
            }

type ValidRegistrationForm =
    private { Name: string
              Email: string
              Password: string
              PasswordConfirmation: string
            }


type RegistrationApiError =
    | CmdError
    | ApiValidationError


[<RequireQualifiedAccess>]
module private Validation =
    let name value =
        if String.isEmpty value
        then Some(FormError "cannot be blank")
        else None

    let email value =
        let validLength = String.length value > 2
        let isEmail = String.contains "@" value

        if validLength && isEmail
        then None
        else Some(FormError "must be an email")

    let password value =
        if String.length value >= 8
        then None
        else Some(FormError "must be at least 8 characters long")

    let passwordConfirmation original copy =
        if original = copy
        then None
        else Some(FormError "confirmation mismatched")


[<RequireQualifiedAccess>]
module RegistrationForm =

    let create =
        let newField = { Value = ""; Error = None }

        { ServerError = None
          Name = newField
          Email = newField
          Password = newField
          PasswordConfirmation = newField
        }

    let serverError (form: RegistrationForm): string = FormError.optionToString form.ServerError

    open Fable.SimpleHttp
    open Fable.SimpleJson

    let request (form: ValidRegistrationForm) =
        let body =
            form
            |> Json.stringify
            |> BodyContent.Text

        Http.request "/api/users"
        |> Http.method POST
        |> Http.headers [ Headers.contentType "application/json" ]
        |> Http.content body

    let private removeServerError form = { form with ServerError = None }

    let name (form: RegistrationForm) = form.Name.Value
    let nameError (form: RegistrationForm) = form.Name.Error |> FormError.optionToString
    let updateName (form: RegistrationForm) newValue = { form with Name = Field.setValue newValue form.Name }
    let validateName (form: RegistrationForm) = { form with Name = Field.validate Validation.name form.Name }

    let email (form: RegistrationForm) = form.Email.Value
    let emailError (form: RegistrationForm) = form.Email.Error |> FormError.optionToString
    let updateEmail (form: RegistrationForm) newValue = { form with Email = Field.setValue newValue form.Email }
    let validateEmail (form: RegistrationForm) = { form with Email = Field.validate Validation.email form.Email }

    let password (form: RegistrationForm) = form.Password.Value
    let passwordError (form: RegistrationForm) = form.Password.Error |> FormError.optionToString
    let updatePassword (form: RegistrationForm) newValue = { form with Password = Field.setValue newValue form.Password }
    let validatePassword (form: RegistrationForm) = { form with Password = Field.validate Validation.password form.Password }

    let passwordConfirmation (form: RegistrationForm) = form.PasswordConfirmation.Value
    let passwordConfirmationError (form: RegistrationForm) = form.PasswordConfirmation.Error |> FormError.optionToString
    let updatePasswordConfirmation (form: RegistrationForm) newValue = { form with PasswordConfirmation = Field.setValue newValue form.PasswordConfirmation }
    let validatePasswordConfirmation (form: RegistrationForm) = { form with PasswordConfirmation = Field.validate (Validation.passwordConfirmation form.Password.Value) form.PasswordConfirmation }


    let private isValid (form: RegistrationForm) =
        [ form.Name; form.Email; form.Password; form.PasswordConfirmation ]
        |> List.all Field.isValid

    let validate (form: RegistrationForm): Result<ValidRegistrationForm, RegistrationForm> =
        let validatedForm =
            form
            |> removeServerError
            |> validateName
            |> validateEmail
            |> validatePassword
            |> validatePasswordConfirmation

        if isValid validatedForm
        then Ok { Name = form.Name.Value
                  Email = form.Email.Value
                  Password = form.Password.Value
                  PasswordConfirmation = form.PasswordConfirmation.Value
                }
        else Error validatedForm

    let applyErrors (err: RegistrationApiError) (form: RegistrationForm): RegistrationForm =
        match err with
            | ApiValidationError ->
                { form with ServerError = Some (FormError "Validation on the server failed") }

            | CmdError ->
                { form with ServerError = Some (FormError "An error occured while contacting our server, please try again later.") }
