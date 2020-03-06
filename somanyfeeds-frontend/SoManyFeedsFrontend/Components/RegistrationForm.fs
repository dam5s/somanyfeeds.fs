module SoManyFeedsFrontend.Components.RegistrationForm

open SoManyFeedsDomain
open SoManyFeedsDomain.Registration
open SoManyFeedsFrontend.Support.Http


type RegistrationForm =
    private { ServerError: string option
              FieldErrors: FieldError<ValidationError> list
              Registration: Registration }


[<RequireQualifiedAccess>]
module RegistrationForm =

    let create =
        { ServerError = None
          FieldErrors = []
          Registration =
              { Name = ""
                Email = ""
                Password = ""
                PasswordConfirmation = "" } }

    let request (registration: ValidRegistration) =
        let fields = Registration.fields registration
        HttpRequest.postJson "/api/users"
            {| name = fields.Name
               email = fields.Email
               password = fields.Password
               passwordConfirmation = fields.Password |}

    let private removeServerError form = { form with ServerError = None }

    let private fieldError name (form: RegistrationForm) =
        form.FieldErrors
        |> List.tryFind (fun f -> f.FieldName = name)
        |> Option.map (fun f -> Registration.errorToString f.Error)
        |> Option.defaultValue ""

    let private removeFieldErrors name form =
        { form with FieldErrors = form.FieldErrors |> List.filter (fun e -> e.FieldName <> name) }

    let private addFieldErrors fieldErrors form =
        { form with FieldErrors = form.FieldErrors |> List.append fieldErrors }

    let private addErrorsIfValidationFailed validation form =
        match validation with
        | Ok _ -> form
        | Error fieldErrors -> addFieldErrors fieldErrors form

    let private validateField (fieldName: string) validation form =
        form
        |> removeFieldErrors fieldName
        |> addErrorsIfValidationFailed (validation form.Registration)

    let private updateRegistration updater form = { form with Registration = updater form.Registration }

    let name form = form.Registration.Name
    let nameError = fieldError "name"
    let updateName newValue = updateRegistration (fun r -> { r with Name = newValue })
    let validateName = validateField "name" Registration.nameValidation

    let email form = form.Registration.Email
    let emailError = fieldError "email"
    let updateEmail newValue = updateRegistration (fun r -> { r with Email = newValue })
    let validateEmail = validateField "email" Registration.emailValidation

    let password form = form.Registration.Password
    let passwordError = fieldError "password"
    let updatePassword newValue = updateRegistration (fun r -> { r with Password = newValue })
    let validatePassword = validateField "password" Registration.passwordValidation

    let passwordConfirmation form = form.Registration.PasswordConfirmation
    let passwordConfirmationError = fieldError "passwordConfirmation"
    let updatePasswordConfirmation newValue = updateRegistration (fun r -> { r with PasswordConfirmation = newValue })
    let validatePasswordConfirmation = validateField "passwordConfirmation" Registration.passwordValidation

    let serverError form = form.ServerError


    let private isValid (form: RegistrationForm) =
        Option.isSome form.ServerError || not (List.isEmpty form.FieldErrors)

    let private buildValidRegistration name email password () =
        { Name = name
          Email = email
          Password = password
          PasswordConfirmation = password }

    let private applyValidationErrors form errors =
        form
        |> removeServerError
        |> addFieldErrors errors

    let validate (form: RegistrationForm): Result<ValidRegistration, RegistrationForm> =
        form.Registration
        |> Registration.validate
        |> Result.mapError (applyValidationErrors form)

    let applyRequestError (err: RequestError) (form: RegistrationForm): RegistrationForm =
        { form with ServerError = Some (RequestError.userMessage err) }
