module Damo.Io.Server.ErrorPageHandler

open Giraffe
open Giraffe.ViewEngine

open Damo.Io.Server.LayoutTemplate

let private errorPage title message =
    article
        []
        [ header [] [ h1 [] [ str title ] ]
          section [] [ p [] [ str message ] ]
          nav [] [ a [ _href "/" ] [ str "Go back home" ] ] ]

let private layoutHandler page : HttpHandler =
    fun next ctx ->
        task {
            let layoutTemplate = ctx.GetService<LayoutTemplate>()
            let! view = layoutTemplate.RenderAsync [ page ]
            return! htmlView view next ctx
        }

let private errorHandler statusCode title message : HttpHandler =
    clearResponse
    >=> setStatusCode statusCode
    >=> layoutHandler (errorPage title message)

[<RequireQualifiedAccess>]
module ErrorPageHandler =
    let serverError: HttpHandler = //
        errorHandler 500 "Server Error" "Oh no! Something went wrong!"

    let notFound: HttpHandler = //
        errorHandler 404 "Not found" "Looks like you're lost!"
