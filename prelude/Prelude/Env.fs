#if !FABLE_COMPILER

[<RequireQualifiedAccess>]
module Env
    open System

    let var name =
        Environment.GetEnvironmentVariable(name) |> Option.ofObj

    let varDefault name producer =
        var name |> Option.defaultValue (producer())

    let requireVar name =
        match var name with
        | Some value -> value
        | None -> failwith $"Missing env variable with name %s{name}"

#endif
