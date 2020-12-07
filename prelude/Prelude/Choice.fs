[<RequireQualifiedAccess>]
module Choice
    let defaultValue other choice =
        match choice with
        | Choice1Of2 value -> value
        | Choice2Of2 _ -> other

    let toOption choice =
        match choice with
        | Choice1Of2 value -> Some value
        | Choice2Of2 _ -> None
