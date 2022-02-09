module SoManyFeedsDomain.Registration


type Registration =
    { Name: string
      Email: string
      Password: string
      PasswordConfirmation: string
    }

type ValidRegistration =
    private ValidRegistration of RegistrationFields

and RegistrationFields =
    { Name: string
      Email: string
      Password: string
    }

[<RequireQualifiedAccess>]
module Registration =
    let name =
        { get = fun (r: Registration) -> r.Name
          set = fun value r -> { r with Name = value } }
    let email =
        { get = fun (r: Registration) -> r.Email
          set = fun value r -> { r with Email = value } }
    let password =
        { get = fun (r: Registration) -> r.Password
          set = fun value r -> { r with Password = value } }
    let passwordConfirmation =
        { get = fun (r: Registration) -> r.PasswordConfirmation
          set = fun value r -> { r with PasswordConfirmation = value } }

    type Error =
        | NameCannotBeBlank
        | EmailCannotBeBlank
        | EmailMustResembleAnEmail
        | EmailAlreadyInUse
        | PasswordMustBeAtLeastEightCharacters
        | PasswordConfirmationMismatched

    let errorToString error =
        match error with
        | NameCannotBeBlank -> "Name cannot be blank"
        | EmailCannotBeBlank -> "Email cannot be blank"
        | EmailMustResembleAnEmail -> "Email is invalid"
        | EmailAlreadyInUse -> "Email is already in use"
        | PasswordMustBeAtLeastEightCharacters -> "Password must be at least 8 characters"
        | PasswordConfirmationMismatched -> "Password confirmation does not match"


[<RequireQualifiedAccess>]
module ValidRegistration =
    let fields (ValidRegistration f) = f
    let email (ValidRegistration f) = f.Email


let private error fieldName validationError =
    Error (Validation.error fieldName validationError)

let nameValidation (registration: Registration): Validation<string, Registration.Error> =
    let name = registration.Name |> String.trim

    if String.isEmpty name
        then error "name" Registration.NameCannotBeBlank
        else Ok name

let emailValidation (registration: Registration) =
    let email = registration.Email |> String.trim |> String.toLowerInvariant

    let isEmpty = String.isEmpty email
    let isNotEmail = not (String.contains "@" email)

    if isEmpty
        then error "email" Registration.EmailCannotBeBlank
        else if isNotEmail
            then error "email" Registration.EmailMustResembleAnEmail
            else Ok email

let passwordValidation (registration: Registration) =
    if String.length registration.Password < 8
        then error "password" Registration.PasswordMustBeAtLeastEightCharacters
        else Ok registration.Password

let passwordConfirmationValidation (registration: Registration) =
    if not (String.equals registration.PasswordConfirmation registration.Password)
        then error "passwordConfirmation" Registration.PasswordConfirmationMismatched
        else Ok ()

let private buildValidRegistration name email password () =
    ValidRegistration
        { Name = name
          Email = email
          Password = password
        }


open Validation.Operators

let validate registration =
    buildValidRegistration
        <!> nameValidation registration
        <*> emailValidation registration
        <*> passwordValidation registration
        <*> passwordConfirmationValidation registration
