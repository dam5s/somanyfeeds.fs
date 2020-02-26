module SoManyFeedsServer.UsersApi

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsDomain.Registration
open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsPersistence.UsersService
open SoManyFeedsServer.Api


[<RequireQualifiedAccess>]
module Json =
    let user record =
        {| id = record.Id
           name = record.Name
           email = record.Email |}

    let private errorToString error =
        match error with
        | NameCannotBeBlank -> "Name cannot be blank"
        | EmailCannotBeBlank -> "Email cannot be blank"
        | EmailMustResembleAnEmail -> "Email is invalid"
        | EmailAlreadyInUse -> "Email is already in use"
        | PasswordMustBeAtLeastEightCharacters -> "Password must be at least 8 characters"
        | PasswordConfirmationMismatched -> "Password confirmation does not match"

    let private fieldError error =
        {| fieldName = error.FieldName
           error = errorToString error.Error |}

    let fieldErrors =
        List.map fieldError


let create (createUser: Registration -> Async<UserCreationResult>) (registration: Registration): HttpHandler =
    fun next ctx ->
        task {
            match! createUser registration with
            | CreationSuccess record -> return! jsonResponse 201 (Json.user record) next ctx
            | CreationFailure errors -> return! jsonResponse 400 (Json.fieldErrors errors) next ctx
            | CreationError message -> return! serverErrorResponse message next ctx
        }
