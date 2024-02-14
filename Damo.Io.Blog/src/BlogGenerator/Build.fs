module BlogGenerator.Build

open BlogGenerator.Html
open BlogGenerator.Rss
open BlogGenerator.Config

[<RequireQualifiedAccess>]
module Build =
    open Fake.IO
    open Fake.IO.Globbing.Operators
    open BlogGenerator.Posts

    let private config: Config =
        { Url = "https://blog.damo.io"
          Title = "Damien Le Berrigaud's Blog"
          Author = "Damien Le Berrigaud" }

    let private relativePath subPath = $"./%s{subPath}"
    let private resourcesPath = relativePath "resources"
    let private postsPath = relativePath "posts"
    let private buildPath = relativePath "build"
    let private publicPath = relativePath "build/public"

    let private cleanupBuildDir () =
        Shell.cleanDir buildPath
        Shell.mkdir buildPath
        Shell.mkdir publicPath

    let private copyResources _ =
        let resourceFiles = !! $"%s{resourcesPath}/*"

        Shell.copy publicPath resourceFiles

    let private generatePost post =
        let postDirPath = $"%s{publicPath}/posts/%s{post.Slug}"
        let postFiles = !! $"%s{post.Dir.FullName}/*"

        Shell.mkdir postDirPath
        Shell.copy postDirPath postFiles

        post
        |> Html.postPage config
        |> File.writeString false $"%s{publicPath}/posts/%s{post.Slug}/index.html"

    let private generatePosts posts =
        posts |> List.iter generatePost |> always posts

    let private generateTagPage posts tag =
        let tagPath = $"%s{publicPath}/tags/%s{tag}"

        let tagPosts =
            posts
            |> List.filter (fun p -> p.Tags |> List.contains tag)
            |> List.sortByDescending (fun p -> p.Posted)

        Shell.mkdir tagPath

        tagPosts
        |> Html.tagPage config tag
        |> File.writeString false $"%s{tagPath}/index.html"

    let private generateTagPages posts =
        let tagsPath = $"%s{publicPath}/tags"
        let tags = Posts.tags posts

        Shell.mkdir tagsPath

        tags |> Set.iter (generateTagPage posts) |> always posts

    let private generateIndex posts =
        posts
        |> Html.tableOfContents config
        |> File.writeString false $"%s{publicPath}/index.html"
        |> always posts

    let private generateRssFeed posts =
        posts
        |> Rss.generate config
        |> (fun xml -> xml.Save $"%s{publicPath}/rss.xml")
        |> always posts

    let run _ =
        cleanupBuildDir ()
        copyResources ()

        Posts.loadFrom config postsPath
        |> generatePosts
        |> generateTagPages
        |> generateIndex
        |> generateRssFeed
        |> ignore
