module SoManyFeeds.Tweet exposing (display)

import Html exposing (Attribute, Html, section)
import Html.Attributes exposing (class)
import Json.Encode as Encode
import Regex
import VirtualDom


display : String -> Html msg
display content =
    section [ innerHtml <| createLinks content ] []


innerHtml : String -> Attribute msg
innerHtml =
    VirtualDom.property "innerHTML" << Encode.string


createLinks : String -> String
createLinks =
    createMentionLinks << createSimpleLinks << createHashTagLinks


replaceAll =
    Regex.replace Regex.All


createMentionLinks =
    replaceAll
        (Regex.regex "(^|\\s)@([A-Za-z_]+[A-Za-z0-9_]+)")
        (replaceLink (\s -> "<a href=\"https://twitter.com/" ++ s ++ "\">@" ++ s ++ "</a>"))


createSimpleLinks =
    replaceAll
        (Regex.regex "(^|\\s)(https?://[^\\s]+)")
        (replaceLink (\s -> "<a href=\"" ++ s ++ "\">" ++ s ++ "</a>"))


createHashTagLinks =
    replaceAll
        (Regex.regex "(^|\\s)#([A-Za-z_]+[A-Za-z0-9_]+)")
        (replaceLink (\s -> "<a href=\"https://twitter.com/hashtag/" ++ s ++ "\">#" ++ s ++ "</a>"))


replaceLink : (String -> String) -> Regex.Match -> String
replaceLink linkBuilder match =
    case match.submatches of
        maybeLeading :: maybeTarget :: _ ->
            case maybeTarget of
                Nothing ->
                    match.match

                Just submatch ->
                    Maybe.withDefault "" maybeLeading ++ linkBuilder submatch

        _ ->
            match.match
