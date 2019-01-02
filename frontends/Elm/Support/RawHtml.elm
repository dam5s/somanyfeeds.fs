module Support.RawHtml exposing (fromString)

import Html exposing (Attribute, Html)
import Html.Attributes
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

        Parser.Element "iframe" attrs children ->
            Html.div [] []

        Parser.Element name attrs children ->
            VirtualDom.node name
                (List.map attributeToHtml attrs)
                (List.map nodeToHtml children)

        Parser.Comment _ ->
            Html.div [] []


attributeToHtml ( name, value ) =
    VirtualDom.attribute name value
