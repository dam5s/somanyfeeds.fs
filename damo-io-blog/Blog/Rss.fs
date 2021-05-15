module Blog.Rss

open System
open System.Xml.Linq
open Blog.Posts
open FSharp.Data.Runtime.BaseTypes

[<RequireQualifiedAccess>]
module Rss =

    open FSharp.Data

    type RssProvider = XmlProvider<"Resources/rss.sample.xml">

    let private blogUrl = "https://blog.damo.io"

    let private replaceElementWithElementWithCData (content: string) (oldElement: XElement) =
        let parent = oldElement.Parent
        let newElement = XElement(oldElement.Name)

        oldElement.Remove()
        newElement.Add(XCData(content))
        parent.Add(newElement)

    let private updateDescriptionToCData (post: Post) (item: RssProvider.Item): RssProvider.Item =
        item.XElement.Elements()
        |> Seq.filter (fun e -> e.Name.LocalName = "description")
        |> Seq.tryHead
        |> Option.iter (replaceElementWithElementWithCData post.HtmlContent)
        |> always item

    let private rssItem (post: Post): RssProvider.Item =
        let link = $"{blogUrl}/posts/{post.Slug}"

        RssProvider.Item(
            title = post.Title,
            link = link,
            guid = RssProvider.Guid(isPermaLink = true, value = link),
            pubDate = RssProvider.PubDate(post.Posted),
            description = post.HtmlContent
        ) |> updateDescriptionToCData post

    let generate (posts: Post list) =
        let items =
            posts
            |> List.map rssItem
            |> Array.ofList

        let channel =
            RssProvider.Channel(
                title = "Damien Le Berrigaud's Blog",
                description = "Ramblings about software, coding, architecture...",
                link = blogUrl,
                lastBuildDate = DateTimeOffset.Now,
                items = items
            )

        RssProvider.Rss(version = 2.0m, channel = channel).XElement
