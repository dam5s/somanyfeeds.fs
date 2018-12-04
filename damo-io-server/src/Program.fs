module Server.Program

open System.IO
open FSharp.Control
open FSharp.Control.AsyncSeqExtensions
open FSharp.Data
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.DotLiquid
open Server.Articles.Data
open Server.SslHandler


module App =
    let private updatesSequence : AsyncSeq<Record list> =
        asyncSeq {
            let tenMinutes = 10 * 1000 * 60

            while true do
                yield FeedsProcessor.processFeeds (Feeds.Repository.findAll ())
                do! Async.Sleep tenMinutes
        }


    let backgroundProcessing =
        AsyncSeq.iter
            Articles.Data.Repository.updateAll
            updatesSequence


    let private articlesList =
        Articles.Handlers.list Articles.Data.Repository.findAll


    let handler =
        choose [
            enforceSsl

            path "/" >=> GET >=> articlesList

            GET >=> Files.browseHome
            NOT_FOUND "not found"
        ]


[<EntryPoint>]
let main _ =
    Async.Start App.backgroundProcessing

    let contentRoot = Directory.GetCurrentDirectory ()

    let templatesFolder = Path.Combine (contentRoot, "templates")
    setTemplatesDir templatesFolder
    setCSharpNamingConvention ()

    let publicFolder = Path.Combine (contentRoot, "public")
    let config = { defaultConfig with homeFolder = Some publicFolder }
    startWebServer config App.handler

    0
