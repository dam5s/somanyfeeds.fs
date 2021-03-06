[<RequireQualifiedAccess>]
module SoManyFeedsServer.Auth.Payload

open SoManyFeedsDomain
open SoManyFeedsDomain.User
open System.Collections.Generic

let private tryGet name (payload: IDictionary<string, obj>) f =
    payload.Item(name)
    |> Option.ofObj
    |> Option.bind f

let private tryCastId = tryCast<int64>
let private tryCastName = tryCast<string>

let tryGetUser (payload: IDictionary<string, obj>): User option =
    Option.map2
        User.create
        (tryGet "userId" payload tryCastId)
        (tryGet "userName" payload tryCastName)

let fromUser (user: User) =
    dict [ ("userId", user.Id :> obj)
           ("userName", user.Name :> obj)
         ]
