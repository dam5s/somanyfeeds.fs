module SoManyFeedsServer.UsersDataGateway

open System.Data.Common
open SoManyFeedsServer.Passwords
open SoManyFeedsServer.DataSource
open SoManyFeedsServer.Registration
open AsyncResult.Operators


type UserRecord =
    { Id : int64
      Name : string
      Email : string
      PasswordHash : HashedPassword
    }


let private mapUser (record : DbDataRecord) : UserRecord =
    { Id = record.GetInt64 0
      Name = record.GetString 1
      Email = record.GetString 2
      PasswordHash = record.GetString 3 |> HashedPassword
    }


let findByEmail (dataSource : DataSource) (email : string) : Async<FindResult<UserRecord>> =
    find dataSource
        """ select id, name, email, password_hash
            from users
            where email = @Email
            limit 1
        """
        [ Binding ("@Email", email) ]
        mapUser


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

    query dataSource
        """ insert into users (name, email, password_hash)
            values (@Name, @Email, @PasswordHash)
            returning id
        """
        bindings
        mapping
        <!> (List.first >> Option.get)
