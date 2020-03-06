module SoManyFeedsServer.UsersApi

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open SoManyFeedsDomain
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

    let private fieldError (error: FieldError<ValidationError>) =
        {| fieldName = error.FieldName
           error = Registration.errorToString error.Error |}

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
