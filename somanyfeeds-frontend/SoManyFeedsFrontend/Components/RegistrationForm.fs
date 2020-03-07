module SoManyFeedsFrontend.Components.RegistrationForm

open SoManyFeedsDomain
open SoManyFeedsDomain.Registration
open SoManyFeedsFrontend.Support.Form
open SoManyFeedsFrontend.Support.Http


type RegistrationForm =
    Form<Registration, Registration.Error>


[<RequireQualifiedAccess>]
module RegistrationForm =
    let private initialModel =
        { Name = ""
          Email = ""
          Password = ""
          PasswordConfirmation = "" }

    let create = Form.create initialModel Registration.errorToString

    let request (registration: ValidRegistration) =
        let fields = ValidRegistration.fields registration
        HttpRequest.postJson "/api/users"
            {| name = fields.Name
               email = fields.Email
               password = fields.Password
               passwordConfirmation = fields.Password |}

    let name form = Registration.name (Form.model form)
    let updateName value = Form.update (Registration.setName value)
    let validateName = Form.validateField "name" Registration.nameValidation

    let email form = Registration.email (Form.model form)
    let updateEmail value = Form.update (Registration.setEmail value)
    let validateEmail = Form.validateField "email" Registration.emailValidation

    let password form = Registration.password (Form.model form)
    let updatePassword value = Form.update (Registration.setPassword value)
    let validatePassword = Form.validateField "password" Registration.passwordValidation

    let passwordConfirmation form = Registration.passwordConfirmation (Form.model form)
    let updatePasswordConfirmation value = Form.update (Registration.setPasswordConfirmation value)
    let validatePasswordConfirmation = Form.validateField "passwordConfirmation" Registration.passwordValidation

    let validate (form: RegistrationForm): Result<ValidRegistration, RegistrationForm> =
        form
        |> Form.model
        |> Registration.validate
        |> Result.mapError (Form.applyValidationErrors form)
