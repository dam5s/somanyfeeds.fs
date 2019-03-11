module SoManyFeedsServer.UsersService

open SoManyFeedsServer
open SoManyFeedsServer.UsersDataGateway
open SoManyFeedsServer.Registration
open SoManyFeedsServer.DataSource
open DataContext


type UserCreationResult =
    | CreationSuccess of UserRecord
    | CreationFailure of ValidationErrors
    | CreationError of message:string


let create (dataSource : DataSource) (dataContext : DataContext) (registration : Registration) : Async<UserCreationResult> =
    async {
        match Registration.validate registration with
        | Ok validReg ->
            match! UsersDataGateway.findByEmail dataContext (Registration.email validReg) with
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
                match! UsersDataGateway.create dataSource validReg with
                | Ok record -> return CreationSuccess record
                | Error message -> return CreationError message
        | Error errors ->
            return CreationFailure errors
    }
