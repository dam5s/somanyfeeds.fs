[<RequireQualifiedAccess>]
module Fantomas

let format _ = Proc.exec "dotnet fantomas ."

let check _ = Proc.exec "dotnet fantomas --check ."
