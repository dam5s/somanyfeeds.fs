module SoManyFeedsServer.UsersDataGateway

open SoManyFeedsServer.Passwords
open SoManyFeedsServer.Registration
open SoManyFeedsServer.DataSource
open SoManyFeedsServer


type UserRecord =
    { Id : int64
      Name : string
      Email : string
      PasswordHash : HashedPassword
    }


let private entityToRecord (entity : UserEntity) : UserRecord =
    { Id = entity.Id
      Name = entity.Name
      Email = entity.Email
      PasswordHash = HashedPassword entity.PasswordHash
    }


let findByEmail (email : string) : Async<FindResult<UserRecord>> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for user in ctx.Public.Users do
                where (user.Email = email)
                take 1
            }
            |> Seq.tryHead
            |> Option.map entityToRecord
        }
    }
    |> FindResult.asyncFromAsyncResultOfOption


let create (registration : ValidRegistration) : AsyncResult<UserRecord> =
    asyncResult {
        let fields = Registration.fields registration
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let entity = ctx.Public.Users.Create ()
            entity.Name <- fields.Name
            entity.Email <- fields.Email
            entity.PasswordHash <- Passwords.hashedValue fields.PasswordHash

            ctx.SubmitUpdates ()

            entityToRecord entity
        }
    }
