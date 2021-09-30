[<RequireQualifiedAccess>]
module SoManyFeedsFrontend.Support.Html

open Fable.React
open Fable.React.Props

let link href txt =
    a [ Href href ] [ str txt ]

let extLink href txt =
    a [ Href href; Target "_blank" ] [ str txt ]

let rawInnerHtml (html: string) =
    DangerouslySetInnerHTML { __html = html.Replace("<a href", "<a target=\"_blank\" href") }

type Dispatcher<'msg>(dispatch: 'msg -> unit) =
    member this.OnClick msg = OnClick (fun _ -> dispatch msg)
    member this.OnClickPreventingDefault msg = OnClick (fun e -> e.preventDefault(); dispatch msg)
    member this.OnSubmit msg = OnSubmit (fun event -> event.preventDefault(); dispatch msg)
    member this.OnChange msg = OnChange (fun event -> dispatch (msg event.Value))
    member this.OnBlur msg = OnBlur (fun _ -> dispatch msg)
