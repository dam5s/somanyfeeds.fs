module Support.RawHtml exposing (fromString, parseEntities)

import Html exposing (Html)
import Html.Attributes exposing (target)
import Html.Parser as Parser
import VirtualDom exposing (Attribute)


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


nodeToHtml : Bool -> Parser.Node -> List (Html msg)
nodeToHtml isTopLevel node =
    case node of
        Parser.Text text ->
            let
                cleanedText =
                    String.replace "&quot;" "\"" text
            in
            if isTopLevel then
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
                        (List.filterMap attributeToHtml attrs ++ [ target "_blank" ])
                        (List.concatMap (nodeToHtml False) children)

                _ ->
                    VirtualDom.node
                        name
                        (List.filterMap attributeToHtml attrs)
                        (List.concatMap (nodeToHtml False) children)


isExcluded : String -> List ParserAttribute -> Bool
isExcluded tag attrs =
    let
        excludedArsTechnicaTracker =
            List.member ( "class", "feedflare" ) attrs
    in
    excludedArsTechnicaTracker


emptyDiv =
    Html.div [] []


attributeToHtml : ( String, String ) -> Maybe (Attribute msg)
attributeToHtml ( name, value ) =
    case name of
        "src" ->
            Just <| VirtualDom.attribute name (String.replace "http://" "https://" value)

        "style" ->
            Nothing

        _ ->
            Just <| VirtualDom.attribute name value
