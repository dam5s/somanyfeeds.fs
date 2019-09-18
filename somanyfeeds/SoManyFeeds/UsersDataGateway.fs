module SoManyFeeds.UsersDataGateway

open SoManyFeeds
open SoManyFeeds.DataSource
open SoManyFeeds.Passwords
open SoManyFeeds.Registration


type UserRecord =
    { Id: int64
      Name: string
      Email: string
      PasswordHash: HashedPassword
    }


let private entityToRecord (entity: UserEntity) =
    { Id = entity.Id
      Name = entity.Name
      Email = entity.Email
      PasswordHash = HashedPassword entity.PasswordHash
    }


let findByEmail email: Async<FindResult<UserRecord>> =
    dataAccessOperation (fun ctx ->
        query {
            for user in ctx.Public.Users do
            where (user.Email = email)
            take 1
        }
        |> Seq.tryHead
        |> Option.map entityToRecord
    )
    |> FindResult.asyncFromAsyncResultOfOption


let create registration: AsyncResult<UserRecord> =
    dataAccessOperation (fun ctx ->
        let fields = Registration.fields registration
        let entity = ctx.Public.Users.Create()
        entity.Name <- fields.Name
        entity.Email <- fields.Email
        entity.PasswordHash <- Passwords.hashedValue fields.PasswordHash

        ctx.SubmitUpdates()

        entityToRecord entity
    )
