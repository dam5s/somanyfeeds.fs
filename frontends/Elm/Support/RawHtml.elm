module Support.RawHtml exposing (fromString, parseEntities)

import Html exposing (Html)
import Html.Attributes exposing (target)
import Html.Parser as Parser
import Regex exposing (Regex)
import VirtualDom


parseEntities : String -> List (Html msg)
parseEntities rawString =
    let
        defaultNode =
            Parser.Text ""
    in
    rawString
        |> Parser.run
        |> Result.map (List.head >> Maybe.withDefault defaultNode)
        |> Result.withDefault defaultNode
        |> nodeToHtml False


fromString : String -> List (Html msg)
fromString rawString =
    rawString
        |> String.replace "</img>" ""
        |> Parser.run
        |> Result.map (List.concatMap (nodeToHtml True))
        |> Result.withDefault []


unformattedRegex : Regex
unformattedRegex =
    "\\n(\\s*)\\n"
        |> Regex.fromString
        |> Maybe.withDefault Regex.never


nodeToHtml : Bool -> Parser.Node -> List (Html msg)
nodeToHtml isTopLevel node =
    case node of
        Parser.Text text ->
            let
                cleanedText =
                    String.replace "&quot;" "\"" text

                isUnformattedText =
                    Regex.contains unformattedRegex cleanedText
            in
            if isTopLevel && isUnformattedText then
                cleanedText
                    |> String.split "\n"
                    |> List.filter (not << String.isEmpty)
                    |> List.map (\t -> Html.p [] [ Html.text t ])

            else if isTopLevel then
                [ Html.p [] [ Html.text cleanedText ] ]

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
                        (List.concatMap (nodeToHtml False) children)

                _ ->
                    VirtualDom.node
                        name
                        (List.map attributeToHtml attrs)
                        (List.concatMap (nodeToHtml False) children)


isExcluded : String -> List ParserAttribute -> Bool
isExcluded tag attrs =
    let
        excludedArsTechnicaTracker =
            List.member ( "class", "feedflare" ) attrs

        slashDotTrackers =
            List.member ( "class", "share_submission" ) attrs
    in
    excludedArsTechnicaTracker || slashDotTrackers


emptyDiv =
    Html.div [] []


attributeToHtml ( name, value ) =
    case name of
        "src" ->
            VirtualDom.attribute name (String.replace "http://" "https://" value)

        _ ->
            VirtualDom.attribute name value
