[<RequireQualifiedAccess>]
module FableFrontend.Support.Html

open Fable.React
open Fable.React.Props

let link href txt =
    a [ Href href ] [ str txt ]

let extLink href txt =
    a [ Href href; Target "_blank" ] [ str txt ]

let rawInnerHtml html =
    // TODO sanitize html
    // add _blank target to links
    // see RawHtml.elm
    DangerouslySetInnerHTML { __html = html }

type Dispatcher<'msg>(dispatch: 'msg -> unit) =
    member this.OnClick msg = OnClick (fun _ -> dispatch msg)
    member this.OnSubmit msg = OnSubmit (fun event -> event.preventDefault(); dispatch msg)
    member this.OnChange msg = OnChange (fun event -> dispatch (msg event.Value))
    member this.OnBlur msg = OnBlur (fun _ -> dispatch msg)
