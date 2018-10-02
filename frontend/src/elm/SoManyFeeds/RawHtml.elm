module SoManyFeeds.RawHtml exposing (fromString)

import Html exposing (Attribute, Html)
import Html.Attributes
import Html.Parser as Parser
import VirtualDom


fromString : String -> Html msg
fromString rawString =
    Parser.run rawString
        |> Result.map parsedNodesToHtml
        |> Result.withDefault (Html.div [] [])


parsedNodesToHtml : List Parser.Node -> Html msg
parsedNodesToHtml =
    List.map nodeToHtml >> Html.div []


nodeToHtml : Parser.Node -> Html msg
nodeToHtml node =
    case node of
        Parser.Text text ->
            Html.text text

        Parser.Element name attrs children ->
            VirtualDom.node name
                (List.map attributeToHtml attrs)
                (List.map nodeToHtml children)

        Parser.Comment _ ->
            Html.div [] []


attributeToHtml ( name, value ) =
    VirtualDom.attribute name value
