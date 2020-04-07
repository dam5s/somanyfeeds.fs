module DamoIOServer.ArticlesHandler

open DamoIOFrontend
open DamoIOFrontend.Article
open FSharp.Control.Tasks.V2.ContextInsensitive
open DamoIOServer.ArticlesDataGateway
open Giraffe
open System
open Time

let private toJson record: Article.Json =
    { Title = record.Title
      Link =  record.Link
      Content =  record.Content
      Date =  (Option.map Posix.milliseconds record.Date)
      Source =  (sprintf "%A" record.Source)
    }

module View =
    open Fable.React
    open Fable.React.Props

    let private Content = HTMLAttr.Content

    let render path (flags: App.Flags) flagsJson: string =
        let model, _ = App.init path flags
        let js = sprintf "DamoIO.StartApp(%s);" flagsJson

        html [ Lang "en" ]
            [ head []
                  [ meta [ CharSet "utf-8" ]
                    meta [ Name "viewport"; Content "width=device-width" ]
                    link [ Rel "stylesheet"; Type "text/css"; Href "/damo-io.css" ]
                    title [] [ str "damo.io - Damien Le Berrigaud's feeds." ]
                  ]
              body []
                  [ div [ Id "damo-io-body" ] [ App.view model ignore ]
                    script [ Src "/damo-io.js" ] []
                    script [ DangerouslySetInnerHTML { __html = js } ] []
                  ]
            ]
        |> Fable.ReactServer.renderToString



let list findAllRecords path: HttpHandler =
    fun next ctx ->
        task {
            let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

            let articles =
                findAllRecords()
                |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)
                |> List.map toJson
                |> List.toArray

            let flags: App.Flags = { Articles = articles }
            let flagsJson = ctx.GetJsonSerializer().SerializeToString(flags)
            let view = View.render (sprintf "/%s" path) flags flagsJson

            return! htmlString view next ctx
        }
