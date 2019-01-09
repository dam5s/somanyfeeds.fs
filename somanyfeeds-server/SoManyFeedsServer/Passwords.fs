module SoManyFeedsServer.Passwords

open BCrypt.Net

type HashedPassword =
    HashedPassword of string


let generateHash =
    BCrypt.HashPassword >> HashedPassword

let verify (clearPassword : string) (HashedPassword passwordHash) : bool =
    BCrypt.Verify (clearPassword, passwordHash)
