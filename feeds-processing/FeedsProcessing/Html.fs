module FeedsProcessing.Html

open Ganss.XSS


let private sanitizer =
    let s = HtmlSanitizer ()
    s.AllowedAttributes.Add("class") |> ignore
    s

let sanitize (html : string) : string =
    sanitizer.Sanitize html
