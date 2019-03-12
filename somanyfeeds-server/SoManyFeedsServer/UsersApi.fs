module SoManyFeedsServer.UsersApi

open Suave
open SoManyFeedsServer
open SoManyFeedsServer.Json
open SoManyFeedsServer.Registration
open SoManyFeedsServer.UsersDataGateway
open SoManyFeedsServer.UsersService


module Encoders =
    open Chiron
    open Chiron.Operators

    let user (record : UserRecord) : Json<unit> =
        Json.write "id" record.Id
        *> Json.write "name" record.Name
        *> Json.write "email" record.Email

    let validationErrors (errors : ValidationErrors) : Json<unit> =
        Json.write "nameError" errors.NameError
        *> Json.write "emailError" errors.EmailError
        *> Json.write "passwordError" errors.PasswordError
        *> Json.write "passwordConfirmationError" errors.PasswordConfirmationError


module Decoders =
    open Chiron
    open Chiron.Operators

    let registration (json : Json) : JsonResult<Registration> * Json =
        let constructor name email password confirmation =
            { Name = name; Email = email; Password = password; PasswordConfirmation = confirmation }

        let decoder =
            constructor
            <!> Json.read "name"
            <*> Json.read "email"
            <*> Json.read "password"
            <*> Json.read "passwordConfirmation"

        decoder json


let create (createUser : Registration -> Async<UserCreationResult>) (registration : Registration) : WebPart =
    fun ctx -> async {
        let! createResult = createUser registration

        match createResult with
        | CreationSuccess record -> return! objectResponse HTTP_201 Encoders.user record ctx
        | CreationFailure errors -> return! objectResponse HTTP_400 Encoders.validationErrors errors ctx
        | CreationError message -> return! serverErrorResponse message ctx
    }
