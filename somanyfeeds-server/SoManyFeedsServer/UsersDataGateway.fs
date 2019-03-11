module SoManyFeedsServer.UsersDataGateway

open System.Data.Common
open SoManyFeedsServer.Passwords
open SoManyFeedsServer.Registration
open SoManyFeedsServer
open AsyncResult.Operators
open DataContext
open SoManyFeedsServer.DataSource


type UserRecord =
    { Id : int64
      Name : string
      Email : string
      PasswordHash : HashedPassword
    }


let private mapUser (id, name, email, passwordHash) : UserRecord =
    { Id = id
      Name = name
      Email = email
      PasswordHash = HashedPassword passwordHash
    }


let findByEmail (dataContext : DataContext) (email : string) : Async<FindResult<UserRecord>> =
    fromOptionResult <| asyncResult {
        let! ctx = dataContext

        return query {
            for user in ctx.Users do
            where (user.Email = email)
            take 1
            select (user.Id, user.Name, user.Email, user.PasswordHash)
        }
        |> Seq.tryHead
        |> Option.map mapUser
    }


let create (dataSource : DataSource) (registration : ValidRegistration) : AsyncResult<UserRecord> =
    let fields = Registration.fields registration

    let bindings =
        [
        Binding ("@Name", fields.Name)
        Binding ("@Email", fields.Email)
        Binding ("@PasswordHash", Passwords.hashedValue fields.PasswordHash)
        ]

    let mapping =
        fun (record : DbDataRecord) ->
            { Id = record.GetInt64(0)
              Name = fields.Name
              Email = fields.Email
              PasswordHash = fields.PasswordHash
            }

    findAll dataSource
        """ insert into users (name, email, password_hash)
            values (@Name, @Email, @PasswordHash)
            returning id
        """
        bindings
        mapping
        <!> (List.first >> Option.get)
