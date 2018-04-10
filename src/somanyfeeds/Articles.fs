module somanyfeeds.Articles

open Giraffe
open GiraffeViewEngine
open Microsoft.AspNetCore.Http


type Record =
    { Text : string }


let private records : Record list =
    [ { Text = "Hello" }
      { Text = "World" } ]


type IRepository =
    abstract member FindAll: Record list

type Repository() =
    interface IRepository with
        member this.FindAll = records
     



type private ViewModel =
    { Text : string }


module private Views =
    open GiraffeViewEngine

    let articleView (article : ViewModel) =
        div [ _class "article" ]
            [ p [] [ rawText article.Text ] ]

    let listView (articles : ViewModel list) =
        section [] <| List.map articleView articles


let private present (record: Record) : ViewModel =
    { Text = record.Text }


let listHandler (layout: XmlNode list -> XmlNode) : HttpHandler =
    fun (next :  HttpFunc) (ctx : HttpContext) ->
        let repository = ctx.GetService<IRepository>()
        let records = repository.FindAll
        let viewModels = List.map present records
        let view = layout [ Views.listView viewModels ]
    
        htmlView view next ctx
