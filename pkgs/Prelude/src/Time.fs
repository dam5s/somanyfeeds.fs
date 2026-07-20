module Time

open System

type Posix = Posix of milliseconds: int64

[<RequireQualifiedAccess>]
module Posix =
    let fromDateTimeOffset (d: DateTimeOffset) = Posix(d.ToUnixTimeMilliseconds())

    let now () = fromDateTimeOffset DateTimeOffset.Now

    let toDateTimeOffset (Posix m) =
        DateTimeOffset.FromUnixTimeMilliseconds(int64 m)
