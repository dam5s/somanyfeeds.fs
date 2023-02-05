[<RequireQualifiedAccess>]
module DamoIoFrontend.Html

open Fable.React.Props

let rawInnerHtml (html: string) =
    DangerouslySetInnerHTML { __html = html.Replace("<a href", "<a target=\"_blank\" href") }
