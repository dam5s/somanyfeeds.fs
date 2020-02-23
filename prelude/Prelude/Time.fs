module Time

open System


type Posix =
    Posix of milliseconds: int64

[<RequireQualifiedAccess>]
module Posix =
    let fromDateTimeOffset (d: DateTimeOffset) = Posix(d.ToUnixTimeMilliseconds())

    let fromDateTime (d: DateTime) = fromDateTimeOffset (DateTimeOffset(d, TimeSpan.Zero))

    let toDateTimeOffset (Posix m) = DateTimeOffset.FromUnixTimeMilliseconds(int64 m)

    let toDateTime p = (toDateTimeOffset p).DateTime

    let milliseconds (Posix m) = m
