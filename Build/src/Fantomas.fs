[<RequireQualifiedAccess>]
module Fantomas

let format _ =
    Proc.exec $"dotnet fantomas --recurse ."

let check _ =
    Proc.exec $"dotnet fantomas --recurse --check ."
