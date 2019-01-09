module SoManyFeedsServer.UsersPersistence

open System.Data.Common
open SoManyFeedsServer.Passwords
open SoManyFeedsServer.DataSource


type UserRecord =
    { Id : int64
      Email : string
      Name : string
      PasswordHash : HashedPassword
    }


let private mapUser (record : DbDataRecord) : UserRecord =
    { Id = record.GetInt64 0
      Email = record.GetString 1
      Name = record.GetString 2
      PasswordHash = record.GetString 3 |> HashedPassword
    }


let findByEmail (dataSource : DataSource) (email : string) : FindResult<UserRecord> =
    find dataSource
        """ select id, email, name, password_hash
            from users
            where email = @Email
            limit 1
        """
        [ Binding ("@Email", email) ]
        mapUser
