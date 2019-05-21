module SoManyFeeds.UsersService

open SoManyFeeds
open SoManyFeeds.UsersDataGateway
open SoManyFeeds.Registration
open SoManyFeeds.DataSource


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of ValidationErrors
    | CreationError of message:string


let create (registration : Registration) : Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            let! findResult = UsersDataGateway.findByEmail (Registration.email validReg)

            match findResult with
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
                let! createResult = UsersDataGateway.create validReg

                match createResult with
                | Ok record -> return CreationSuccess record
                | Error message -> return CreationError message
        | Error errors ->
            return CreationFailure errors
    }
