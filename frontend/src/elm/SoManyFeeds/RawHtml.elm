module SoManyFeeds.RawHtml exposing (fromString)

import Html exposing (Attribute, Html)
import Markdown


fromString : String -> Html msg
fromString =
    let
        defaults =
            Markdown.defaultOptions

        options =
            { defaults | sanitize = False }
    in
    Markdown.toHtmlWith options []
