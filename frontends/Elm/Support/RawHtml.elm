module Support.RawHtml exposing (fromString)

import Html exposing (Html, p)
import Html.Attributes exposing (target)
import Html.Parser as Parser
import VirtualDom


fromString : String -> List (Html msg)
fromString rawString =
    rawString
        |> String.replace "</img>" ""
        |> Parser.run
        |> Result.map (List.concatMap nodeToHtml)
        |> Result.withDefault []


nodeToHtml : Parser.Node -> List (Html msg)
nodeToHtml node =
    case node of
        Parser.Text text ->
            let
                cleanedText =
                    String.replace "&quot;" "\"" text
            in
            if String.contains "\n\n" cleanedText then
                cleanedText
                    |> String.split "\n\n"
                    |> List.map (\t -> Html.p [] [ Html.text t ])

            else
                [ Html.text cleanedText ]

        Parser.Element name attrs children ->
            [ elementToHtml name attrs children ]

        Parser.Comment _ ->
            [ emptyDiv ]


type alias ParserAttribute =
    ( String, String )


elementToHtml : String -> List ParserAttribute -> List Parser.Node -> Html msg
elementToHtml name attrs children =
    case isExcluded name attrs of
        True ->
            emptyDiv

        False ->
            case name of
                "a" ->
                    Html.a
                        (List.map attributeToHtml attrs ++ [ target "_blank" ])
                        (List.concatMap nodeToHtml children)

                _ ->
                    VirtualDom.node
                        name
                        (List.map attributeToHtml attrs)
                        (List.concatMap nodeToHtml children)


excludedTags =
    [ "iframe", "embed", "object" ]


isExcluded : String -> List ParserAttribute -> Bool
isExcluded tag attrs =
    let
        excludedTag =
            List.member tag excludedTags

        excludedArsTechnicaTracker =
            List.member ( "class", "feedflare" ) attrs

        slashDotTrackers =
            List.member ( "class", "share_submission" ) attrs
    in
    excludedTag || excludedArsTechnicaTracker || slashDotTrackers


emptyDiv =
    Html.div [] []


attributeToHtml ( name, value ) =
    case name of
        "src" ->
            VirtualDom.attribute name (String.replace "http://" "https://" value)

        _ ->
            VirtualDom.attribute name value
