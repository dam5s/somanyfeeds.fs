module SoManyFeeds.Registration

open Passwords


type Registration =
    { Name : string
      Email : string
      Password : string
      PasswordConfirmation : string
    }


type RegistrationFields =
    { Name : string
      Email : string
      PasswordHash : HashedPassword
    }


type ValidRegistration =
    private ValidRegistration of RegistrationFields


let fields (ValidRegistration f) = f
let email (ValidRegistration f) = f.Email


type ValidationErrors =
    { NameError : string option
      EmailError : string option
      PasswordError : string option
      PasswordConfirmationError : string option
    }


let private nameValidation (registration : Registration) : string option =
    if String.isEmpty registration.Name
    then Some "cannot be blank"
    else None


let private emailValidation (registration : Registration) : string option =
    let isEmpty = String.isEmpty registration.Email
    let isNotEmail = not (String.contains "@" registration.Email)

    if isEmpty || isNotEmail
    then Some "must be an email"
    else None


let private passwordValidation (registration : Registration) : string option =
    if String.length registration.Password < 8
    then Some "must be at least 8 characters long"
    else None


let private passwordConfirmationValidation (registration : Registration) : string option =
    if not (String.equals registration.PasswordConfirmation registration.Password)
    then Some "confirmation mismatched"
    else None


let private anyError (errors : ValidationErrors) : bool =
    [ errors.NameError; errors.EmailError; errors.PasswordError; errors.PasswordConfirmationError ]
    |> List.exists Option.isSome


let private buildFields (registration : Registration) : RegistrationFields =
    { Name = registration.Name
             |> String.trim
      Email = registration.Email
              |> String.trim
              |> String.toLowerInvariant
      PasswordHash = Passwords.generateHash registration.Password
    }


let validate (registration : Registration) : Result<ValidRegistration, ValidationErrors> =
    let errors =
        { NameError = nameValidation registration
          EmailError = emailValidation registration
          PasswordError = passwordValidation registration
          PasswordConfirmationError = passwordConfirmationValidation registration
        }

    if anyError errors
    then Error errors
    else buildFields registration
         |> ValidRegistration
         |> Ok
