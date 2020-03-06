module SoManyFeedsPersistence.UsersDataGateway

open SoManyFeedsPersistence.DataSource
open SoManyFeedsDomain


module private Password =
    open BCrypt.Net

    let hash = BCrypt.HashPassword
    let verify clearPassword hash =
        BCrypt.Verify (clearPassword, hash)

type UserRecord =
    { Id: int64
      Name: string
      Email: string
    }

let private entityToRecord (entity: UserEntity) =
    { Id = entity.Id
      Name = entity.Name
      Email = entity.Email
    }

let private entityToRecordIfPasswordMatch password (entity: UserEntity) =
    if Password.verify password entity.PasswordHash
        then Some (entityToRecord entity)
        else None


let findByEmailAndPassword email password: Async<FindResult<UserRecord>> =
    dataAccessOperation (fun ctx ->
        query {
            for user in ctx.Public.Users do
            where (user.Email = email)
            take 1
        }
        |> Seq.tryHead
        |> Option.bind (entityToRecordIfPasswordMatch password)
    )
    |> FindResult.asyncFromAsyncResultOfOption


let create registration: AsyncResult<UserRecord> =
    dataAccessOperation (fun ctx ->
        let fields = Registration.fields registration
        let entity = ctx.Public.Users.Create()
        entity.Name <- fields.Name
        entity.Email <- fields.Email
        entity.PasswordHash <- Password.hash fields.Password

        ctx.SubmitUpdates()

        entityToRecord entity
    )


let exists email: Async<ExistsResult> =
    dataAccessOperation (fun ctx ->
        query {
            for user in ctx.Public.Users do
            exists (user.Email = email)
        }
    )
    |> ExistsResult.asyncFromAsyncResultOfBoolean
