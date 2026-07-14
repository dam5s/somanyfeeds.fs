module Damo.Io.Server.ArticlesHandler

open Giraffe
open System
open Time

open Damo.Io.Server.Article
open Damo.Io.Server.ArticleListTemplate
open Damo.Io.Server.ArticlesRepository
open Damo.Io.Server.IHttpHandler
open Damo.Io.Server.LayoutTemplate

let rec private mergeConsecutiveArticles now = function
    | [] -> []
    | [x] -> [x]
    | first :: second :: rest ->
        if first.Title.IsSome && first.Title = second.Title then
            let merged =
                { first with
                    Date =
                        Some
                            (max
                                (Option.defaultValue now first.Date)
                                (Option.defaultValue now second.Date))
                    Content =
                        second.Content + first.Content }
            mergeConsecutiveArticles now (merged :: rest)
        else
            first :: mergeConsecutiveArticles now (second :: rest)

type ListArticlesHandler(articlesRepo: ArticlesRepository, layoutTemplate: LayoutTemplate) =
    interface IHttpHandler with
        member _.Handle(next, ctx) =
            task {
                let! articles = articlesRepo.FindAllAsync()

                let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

                let sortedArticles =
                    articles |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

                let mergedArticles = mergeConsecutiveArticles now sortedArticles

                let! view = ArticleListTemplate.render mergedArticles |> layoutTemplate.RenderAsync

                return! htmlView view next ctx
            }
