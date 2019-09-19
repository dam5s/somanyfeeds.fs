module SoManyFeeds.Registration

open Passwords


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
      PasswordHash: HashedPassword
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


let private nameValidation (registration: Registration): Validation<string, ValidationError> =
    let name = registration.Name |> String.trim

    if String.isEmpty name
        then Error [NameCannotBeBlank]
        else Ok name

let private emailValidation (registration: Registration) =
    let email = registration.Email |> String.trim |> String.toLowerInvariant

    let isEmpty = String.isEmpty email
    let isNotEmail = not (String.contains "@" email)

    if isEmpty
        then Error [EmailCannotBeBlank]
        else if isNotEmail
            then Error [EmailMustResembleAnEmail]
            else Ok email

let private passwordValidation registration =
    if String.length registration.Password < 8
        then Error [PasswordMustBeAtLeastEightCharacters]
        else if not (String.equals registration.PasswordConfirmation registration.Password)
            then Error [PasswordConfirmationMismatched]
            else Ok (Passwords.generateHash registration.Password)

let private buildRegistration name email passwordHash =
    ValidRegistration
        { Name = name
          Email = email
          PasswordHash = passwordHash
        }


open Validation.Operators

let validate registration =
    buildRegistration
        <!> nameValidation registration
        <*> emailValidation registration
        <*> passwordValidation registration
