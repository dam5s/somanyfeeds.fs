module Time

open System


type Posix =
    private Posix of milliseconds:int64

[<RequireQualifiedAccess>]
module Posix =
    let fromDateTimeOffset (d : DateTimeOffset) = Posix (d.ToUnixTimeMilliseconds ())

    let fromDateTime (d : DateTime) = fromDateTimeOffset (DateTimeOffset (d, TimeSpan.Zero))

    let toDateTimeOffset (Posix m) = DateTimeOffset.FromUnixTimeMilliseconds m

    let toDateTime (p : Posix) = (toDateTimeOffset p).DateTime

    let milliseconds (Posix m) = m
