﻿module BlogGenerator.Tasks

open BlogGenerator.Html
open BlogGenerator.Rss

[<RequireQualifiedAccess>]
module Tasks =
    open Fake.IO
    open Fake.IO.Globbing.Operators
    open BlogGenerator.Posts
    open BlogGenerator.Scss

    let private relativePath subPath = $"./%s{subPath}"
    let private assetsPath = relativePath "Assets"
    let private postsPath = relativePath "Posts"
    let private buildPath = relativePath "build"
    let private publicPath = relativePath "build/public"

    let private cleanupBuildDir () =
        Shell.cleanDir buildPath
        Shell.mkdir buildPath
        Shell.mkdir publicPath

    let private generateScss _ =
        $"%s{assetsPath}/app.scss"
        |> Scss.convert
        |> File.writeString false $"%s{publicPath}/app.css"

    let private generatePost post =
        let postDirPath = $"%s{publicPath}/posts/%s{post.Slug}"
        let postFiles = !! $"%s{post.Dir.FullName}/*"

        Shell.mkdir postDirPath
        Shell.copy postDirPath postFiles

        post
        |> Html.postPage
        |> File.writeString false $"%s{publicPath}/posts/%s{post.Slug}/index.html"

    let private generatePosts posts =
        posts
        |> List.iter generatePost
        |> always posts

    let private generateTagPage posts tag =
        let tagPath = $"%s{publicPath}/tags/%s{tag}"
        let tagPosts =
            posts
            |> List.filter (fun p -> p.Tags |> List.contains tag)
            |> List.sortByDescending (fun p -> p.Posted)

        Shell.mkdir tagPath

        tagPosts
        |> Html.tagPage tag
        |> File.writeString false $"%s{tagPath}/index.html"

    let private generateTagPages posts =
        let tagsPath = $"%s{publicPath}/tags"
        let tags = Posts.tags posts

        Shell.mkdir tagsPath

        tags
        |> Set.iter (generateTagPage posts)
        |> always posts

    let private generateIndex posts =
        posts
        |> Html.tableOfContents
        |> File.writeString false $"%s{publicPath}/index.html"
        |> always posts

    let private generateRssFeed posts =
        posts
        |> Rss.generate
        |> (fun xml -> xml.Save $"%s{publicPath}/rss.xml")
        |> always posts

    let private generateHerokuConfig _ =
        """{"root": "public/"}"""
        |> File.writeString false $"%s{buildPath}/static.json"

    let build _ =
        cleanupBuildDir ()
        generateScss ()

        Posts.loadFrom postsPath
        |> generatePosts
        |> generateTagPages
        |> generateIndex
        |> generateRssFeed
        |> generateHerokuConfig
