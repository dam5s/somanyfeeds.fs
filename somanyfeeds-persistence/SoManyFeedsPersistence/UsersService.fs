module SoManyFeedsPersistence.UsersService

open SoManyFeedsPersistence.DataSource
open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsDomain.Registration
open SoManyFeedsDomain


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of FieldError<Registration.Error> list
    | CreationError of Explanation


let create registration: Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            match! UsersDataGateway.exists (ValidRegistration.email validReg) with
            | Found _ ->
                return CreationFailure (Validation.error "email" Registration.EmailAlreadyInUse)
            | FindError explanation ->
                return CreationError explanation
            | NotFound ->
                match! UsersDataGateway.create validReg with
                | Ok record -> return CreationSuccess record
                | Error explanation -> return CreationError explanation
        | Error errors ->
            return CreationFailure errors
    }
