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


let fields (ValidRegistration f) = f
let email (ValidRegistration f) = f.Email


type ValidationError =
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


let private error fieldName validationError =
    Error (Validation.error fieldName validationError)

let nameValidation (registration: Registration): Validation<string, ValidationError> =
    let name = registration.Name |> String.trim

    if String.isEmpty name
        then error "name" NameCannotBeBlank
        else Ok name

let emailValidation (registration: Registration) =
    let email = registration.Email |> String.trim |> String.toLowerInvariant

    let isEmpty = String.isEmpty email
    let isNotEmail = not (String.contains "@" email)

    if isEmpty
        then error "email" EmailCannotBeBlank
        else if isNotEmail
            then error "email" EmailMustResembleAnEmail
            else Ok email

let passwordValidation (registration: Registration) =
    if String.length registration.Password < 8
        then error "password" PasswordMustBeAtLeastEightCharacters
        else Ok registration.Password

let passwordConfirmationValidation (registration: Registration) =
    if not (String.equals registration.PasswordConfirmation registration.Password)
        then error "passwordConfirmation" PasswordConfirmationMismatched
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
