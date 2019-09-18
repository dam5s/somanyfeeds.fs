module SoManyFeeds.UsersService

open SoManyFeeds
open SoManyFeeds.DataSource
open SoManyFeeds.Registration
open SoManyFeeds.UsersDataGateway


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of ValidationErrors
    | CreationError of message: string


let create registration: Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            match! UsersDataGateway.findByEmail (Registration.email validReg) with
            | Found _ ->
                return CreationFailure
                    { NameError = None
                      EmailError = Some "email is already in use"
                      PasswordError = None
                      PasswordConfirmationError = None
                    }
            | FindError message ->
                return CreationError message
            | _ ->
                match! UsersDataGateway.create validReg with
                | Ok record -> return CreationSuccess record
                | Error message -> return CreationError message
        | Error errors ->
            return CreationFailure errors
    }
