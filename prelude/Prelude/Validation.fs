namespace global


type Validation<'Success, 'Failure> =
    Result<'Success, 'Failure list>


[<RequireQualifiedAccess>]
module Validation =

    let map f (x: Validation<_, _>): Validation<_, _> =
        Result.map f x

    let bind f (x: Validation<_, _>): Validation<_, _> =
        Result.bind f x

    let apply (fV: Validation<_, _>) (xV: Validation<_, _>): Validation<_, _> =
        match fV, xV with
        | Ok f, Ok x -> Ok(f x)
        | Error errs1, Ok _ -> Error errs1
        | Ok _, Error errs2 -> Error errs2
        | Error errs1, Error errs2 -> Error(errs1 @ errs2)

    module Operators =
        let (<!>) = map
        let (<*>) = apply
