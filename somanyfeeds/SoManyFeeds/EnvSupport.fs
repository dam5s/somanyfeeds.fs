[<AutoOpen>]
module SoManyFeeds.EnvSupport

[<RequireQualifiedAccess>]
module Env =
    open System

    let var name =
        Environment.GetEnvironmentVariable(name) |> Option.ofObj

    let varDefault name producer =
        var name |> Option.defaultValue (producer())
