module somanyfeeds.Articles.Handlers

open Giraffe
open GiraffeViewEngine


type private ViewModel =
    { Text : string }


module private Views =
    open GiraffeViewEngine

    let articleView (article : ViewModel) = 
        div [ _class "article" ]
            [ p [] [ rawText article.Text ] ]

    let listView (articles : ViewModel list) = 
        section [] <| List.map articleView articles


let private articles : ViewModel list =
    [ { Text = "Hello" }
      { Text = "World" } ]


let listHandler (layout : (XmlNode list -> XmlNode)) =
    htmlView (layout [ Views.listView articles ])
