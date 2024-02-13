module BlogGenerator.Rss

open System
open System.Xml.Linq
open BlogGenerator.Posts
open BlogGenerator.Config

[<RequireQualifiedAccess>]
module Rss =

    let element name attributes (children: XNode list) : XElement =
        let xe = XElement(XName.Get(name))

        for (name, value) in attributes do
            xe.SetAttributeValue(XName.Get(name), value)

        for child in children do
            xe.Add(child)

        xe

    let cdata (content: string) = XCData(content)
    let str (content: string) = XText(content)

    let dateStr (date: DateTimeOffset) =
        str (date.DateTime.ToString "yyyy-MM-ddTHH:mm:sszzz")

    let private rssItem (config: Config) (post: Post) =
        let link = $"{config.Url}/posts/{post.Slug}"

        element
            "item"
            []
            [ element "title" [] [ str post.Title ]
              element "link" [] [ str link ]
              element "guid" [ "isPermaLink", "true" ] [ str link ]
              element "pubDate" [] [ dateStr post.Posted ]
              element "description" [] [ cdata post.RssContent ] ]

    let generate (config: Config) (posts: Post list) =
        let items = posts |> List.map (rssItem config) |> List.choose tryUnbox<XNode>

        element
            "rss"
            [ "version", "2.0" ]
            [ element
                  "channel"
                  []
                  ([ element "title" [] [ str "Damien Le Berrigaud's Blog" ]
                     element "description" [] [ str "Ramblings about software, coding, architecture..." ]
                     element "link" [] [ str config.Url ]
                     element "lastBuildDate" [] [ dateStr DateTimeOffset.Now ] ]
                   @ items) ]
