module Support.RawHtml exposing (fromString)

import Html exposing (Attribute, Html)
import Html.Attributes exposing (target)
import Html.Parser as Parser
import VirtualDom


fromString : String -> List (Html msg)
fromString rawString =
    Parser.run rawString
        |> Result.map (List.map nodeToHtml)
        |> Result.withDefault []


nodeToHtml : Parser.Node -> Html msg
nodeToHtml node =
    case node of
        Parser.Text text ->
            Html.text (String.replace "&quot;" "\"" text)

        Parser.Element "a" attrs children ->
            Html.a
                (List.map attributeToHtml attrs ++ [ target "_blank" ])
                (List.map nodeToHtml children)

        Parser.Element "iframe" _ _ ->
            Html.div [] []

        Parser.Element "embed" _ _ ->
            Html.div [] []

        Parser.Element "object" _ _ ->
            Html.div [] []

        Parser.Element name attrs children ->
            VirtualDom.node name
                (List.map attributeToHtml attrs)
                (List.map nodeToHtml children)

        Parser.Comment _ ->
            Html.div [] []


attributeToHtml ( name, value ) =
    VirtualDom.attribute name value
