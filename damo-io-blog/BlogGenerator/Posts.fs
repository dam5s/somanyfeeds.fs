module BlogGenerator.Posts

open System
open System.IO
open Metadata
open Markdown
open Config

type Post =
    { Title: string
      Slug: string
      Posted: DateTimeOffset
      Tags: string list
      HtmlContent: string
      RssContent: string
      Dir: DirectoryInfo }

[<RequireQualifiedAccess>]
module Post =
    let path post =
        $"/posts/%s{post.Slug}"

    let displayDate post =
        let date = post.Posted.DateTime
        let month = date.ToString "MMMM"
        let day = date.ToString "dd"
        let year = date.ToString "yyyy"

        $"%s{month} %s{day}, %s{year}"

module Posts =
    open OptionBuilder

    let private tryReadFromDir (config: Config) (dir: DirectoryInfo) =
        option {
            let urlPrefix = $"{config.Url}/posts/{dir.Name}/"

            let! metadata = Metadata.tryGet dir
            let! article = Markdown.tryGet urlPrefix dir

            let post =
                { Title = article.Title
                  Slug = dir.Name
                  Posted = metadata.Posted
                  Tags = metadata.Tags
                  HtmlContent = article.HtmlContent
                  RssContent = article.RssContent
                  Dir = dir }

            return! Some post
        }

    let loadFrom config dirPath =
        let dir = DirectoryInfo(dirPath)
        let dirs = dir.GetDirectories()

        dirs
        |> Array.choose (tryReadFromDir config)
        |> Array.toList

    let tags posts =
        posts
        |> List.map (fun p -> p.Tags)
        |> List.concat
        |> Set.ofList
