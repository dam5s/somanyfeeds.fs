module SoManyFeedsServer.Auth.Admin

open Giraffe
open SoManyFeedsServer.Auth
open SoManyFeedsDomain.User

type Admin =
    private Admin of User

[<RequireQualifiedAccess>]
module Admin =

    let private adminId =
        Env.var "ADMIN_ID"

    let private isAdmin (user: User) =
        Some $"%d{user.Id}" = adminId

    let private tryPromoteUser (user: User): Admin option =
        Option.ofBoolean (Admin user) (isAdmin user)

    let authenticate (withAdmin: Admin -> HttpHandler): HttpHandler =
        fun next ctx ->
            ctx
            |> Session.getUser
            |> Option.bind tryPromoteUser
            |> Option.map (fun admin -> withAdmin admin next ctx)
            |> Option.defaultValue skipPipeline
