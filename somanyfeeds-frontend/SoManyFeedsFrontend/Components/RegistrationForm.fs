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

    open Optics.Operators

    let name: Lens<RegistrationForm, string> = Form.model >-> Registration.name
    let validateName = Form.validateField "name" Registration.nameValidation

    let email: Lens<RegistrationForm, string> = Form.model >-> Registration.email
    let validateEmail = Form.validateField "email" Registration.emailValidation

    let password: Lens<RegistrationForm, string> = Form.model >-> Registration.password
    let validatePassword = Form.validateField "password" Registration.passwordValidation

    let passwordConfirmation: Lens<RegistrationForm, string> = Form.model >-> Registration.passwordConfirmation
    let validatePasswordConfirmation = Form.validateField "passwordConfirmation" Registration.passwordValidation

    let validate (form: RegistrationForm): Result<ValidRegistration, RegistrationForm> =
        form
        |> Form.model.get
        |> Registration.validate
        |> Result.mapError (Form.applyValidationErrors form)
