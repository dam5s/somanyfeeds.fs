module FeedsProcessing.Html

open Ganss.XSS

let private disallow attr (s: HtmlSanitizer) =
    s.AllowedAttributes.Remove(attr) |> ignore
    s

let private allow attr (s: HtmlSanitizer) =
    s.AllowedAttributes.Add(attr) |> ignore
    s

let private sanitizer =
    HtmlSanitizer()
    |> allow "class"
    |> disallow "width"
    |> disallow "height"
    |> disallow "style"

let sanitize =
    sanitizer.Sanitize
