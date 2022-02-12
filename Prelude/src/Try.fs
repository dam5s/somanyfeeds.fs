[<AutoOpen>]
module Try

open System

module Try =
    let private error description (ex: Exception) =
        let msg = sprintf "%s error: %s" description (ex.Message.Trim())
        Error.create msg ex

    let value description func =
        try func() |> Ok
        with ex -> error description ex

    let result description func =
        try func()
        with ex -> error description ex
