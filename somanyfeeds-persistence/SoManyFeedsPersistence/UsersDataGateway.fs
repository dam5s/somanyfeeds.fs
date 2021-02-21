module SoManyFeedsPersistence.UsersDataGateway

open SoManyFeedsPersistence.DataSource
open SoManyFeedsDomain.Registration
open Time

module private Password =
    open BCrypt.Net

    let hash = BCrypt.HashPassword
    let verify clearPassword hash =
        BCrypt.Verify (clearPassword, hash)

type UserRecord =
    { Id: int64
      Name: string
      Email: string
      LastLogin: Posix option
    }

let private entityToRecord (entity: UserEntity) =
    { Id = entity.Id
      Name = entity.Name
      Email = entity.Email
      LastLogin = entity.LastLogin |> Option.map Posix.fromDateTime
    }

let private updateLastLogin (ctx: DataContext) (entity: UserEntity) =
    entity.LastLogin <-
        Posix.now ()
        |> Posix.toDateTime
        |> Some
    ctx.SubmitUpdates ()
    entity


let loginByEmailAndPassword email password =
    dataAccessOperation (fun ctx ->
        query {
            for user in ctx.Public.Users do
            where (user.Email = email)
            take 1
        }
        |> Seq.tryHead
        |> Option.bind (fun entity ->
            if Password.verify password entity.PasswordHash
                then
                    entity
                    |> updateLastLogin ctx
                    |> entityToRecord
                    |> Some
                else
                    None
        )
    )
    |> FindResult.asyncFromAsyncResultOfOption


let listUsers: AsyncResult<UserRecord seq> =
    dataAccessOperation (fun ctx ->
        query {
            for user in ctx.Public.Users do
            take 500
            select user
        }
        |> Seq.map entityToRecord
    )


let create registration: AsyncResult<UserRecord> =
    dataAccessOperation (fun ctx ->
        let fields = ValidRegistration.fields registration
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
