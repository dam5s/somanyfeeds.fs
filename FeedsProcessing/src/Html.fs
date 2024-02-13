module FeedsProcessing.Html

open Ganss.Xss

let private disallow attr (s: HtmlSanitizer) =
    s.AllowedAttributes.Remove(attr) |> ignore
    s

let private allow attr (s: HtmlSanitizer) =
    s.AllowedAttributes.Add(attr) |> ignore
    s

let private removeUrlsContaining (substring: string) (s: HtmlSanitizer) =
    s.FilterUrl.Add(fun (e: FilterUrlEventArgs) ->
        if e.OriginalUrl.Contains(substring) then
            e.SanitizedUrl <- null
        else
            ())

    s

let private sanitizer =
    HtmlSanitizer()
    |> allow "class"
    |> disallow "width"
    |> disallow "height"
    |> disallow "style"
    |> removeUrlsContaining "/tracking"

let sanitize = sanitizer.Sanitize
