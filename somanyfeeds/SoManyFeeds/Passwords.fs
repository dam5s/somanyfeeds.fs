module SoManyFeeds.Passwords

open BCrypt.Net

type HashedPassword =
    HashedPassword of string


let hashedValue (HashedPassword value) = value

let generateHash =
    BCrypt.HashPassword >> HashedPassword

let verify clearPassword (HashedPassword passwordHash) =
    BCrypt.Verify (clearPassword, passwordHash)
