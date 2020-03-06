module SoManyFeedsPersistence.UsersService

open SoManyFeedsPersistence.DataSource
open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsDomain.Registration
open SoManyFeedsDomain


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of FieldError<ValidationError> list
    | CreationError of message: string


let create registration: Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            match! UsersDataGateway.exists (Registration.email validReg) with
            | Found _ ->
                return CreationFailure (Validation.error "email" EmailAlreadyInUse)
            | FindError message ->
                return CreationError message
            | NotFound ->
                match! UsersDataGateway.create validReg with
                | Ok record -> return CreationSuccess record
                | Error message -> return CreationError message
        | Error errors ->
            return CreationFailure errors
    }
