module SoManyFeedsServer.UsersService

open SoManyFeedsServer
open SoManyFeedsServer.UsersDataGateway
open SoManyFeedsServer.Registration
open SoManyFeedsServer.DataSource


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of ValidationErrors
    | CreationError of message:string


let create (dataContext : DataContext) (registration : Registration) : Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            let! findResult = UsersDataGateway.findByEmail dataContext (Registration.email validReg)

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
                let! createResult = UsersDataGateway.create dataContext validReg

                match createResult with
                | Ok record -> return CreationSuccess record
                | Error message -> return CreationError message
        | Error errors ->
            return CreationFailure errors
    }
