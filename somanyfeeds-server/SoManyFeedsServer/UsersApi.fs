module SoManyFeedsServer.UsersApi

open SoManyFeeds.Registration
open SoManyFeeds.UsersDataGateway
open SoManyFeeds.UsersService
open SoManyFeedsServer
open SoManyFeedsServer.Json
open Suave


module Encoders =
    open Chiron
    open Chiron.Operators

    let user record =
        Json.write "id" record.Id
        *> Json.write "name" record.Name
        *> Json.write "email" record.Email

    let private errorToString error =
        match error with
        | NameCannotBeBlank -> "Name cannot be blank"
        | EmailCannotBeBlank -> "Email cannot be blank"
        | EmailMustResembleAnEmail -> "Email is invalid"
        | EmailAlreadyInUse -> "Email is already in use"
        | PasswordMustBeAtLeastEightCharacters -> "Password must be at least 8 characters"
        | PasswordConfirmationMismatched -> "Password confirmation does not match"

    let errorsToString errors =
        List.map errorToString errors



module Decoders =
    open Chiron
    open Chiron.Operators

    let registration json =
        let constructor name email password confirmation =
            { Name = name; Email = email; Password = password; PasswordConfirmation = confirmation }

        let decoder =
            constructor
            <!> Json.read "name"
            <*> Json.read "email"
            <*> Json.read "password"
            <*> Json.read "passwordConfirmation"

        decoder json


let create createUser (registration: Registration): WebPart =
    fun ctx -> async {
        match! createUser registration with
        | CreationSuccess record -> return! objectResponse HTTP_201 Encoders.user record ctx
        | CreationFailure errors -> return! simpleListResponse HTTP_400 (Encoders.errorsToString errors) ctx
        | CreationError message -> return! serverErrorResponse message ctx
    }
