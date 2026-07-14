module ListArticlesTests

open Xunit
open System
open Time

open Damo.Io.Server.ListArticles
open FeedsPersistence.ArticleRecord

let private daysSinceEpoch days =
    DateTimeOffset(1970, 1, 1 + days, 0, 0, 0, TimeSpan.Zero)
    |> Posix.fromDateTimeOffset

let private testArticle =
    { Title = Some "Title"
      Link = None
      Content = "content"
      Media = None
      Date = Some(daysSinceEpoch 1)
      FeedName = "Feed" }

[<Fact>]
let ``mergeConsecutiveArticles with some consecutive articles sharing title and feed name`` () =
    let a =
        { testArticle with
            Date = Some(daysSinceEpoch 1)
            Title = Some "A"
            Content = "a" }

    let b1 =
        { testArticle with
            Date = Some(daysSinceEpoch 2)
            Title = Some "B"
            Content = "b1" }

    let b2 =
        { testArticle with
            Date = Some(daysSinceEpoch 3)
            Title = Some "B"
            Content = "b2" }

    let b3 =
        { testArticle with
            Date = Some(daysSinceEpoch 4)
            Title = Some "B"
            Content = "b3" }

    let c =
        { testArticle with
            Date = Some(daysSinceEpoch 5)
            Title = Some "C"
            Content = "c" }

    let result =
        ListArticles.mergeConsecutiveArticles (Posix.now ()) [ a; b1; b2; b3; c ]

    Assert.Equal(3, result.Length)
    let bMerged = result |> List.find (fun x -> x.Title = Some "B")
    Assert.Equal("b3b2b1", bMerged.Content)
    Assert.Equal(Some(daysSinceEpoch 4), bMerged.Date)

[<Fact>]
let ``mergeConsecutiveArticles for an empty list`` () =
    let result = ListArticles.mergeConsecutiveArticles (Posix.now ()) []
    Assert.Equal<ArticleRecord>([], result)

[<Fact>]
let ``mergeConsecutiveArticles for a single article`` () =
    let result = ListArticles.mergeConsecutiveArticles (Posix.now ()) [ testArticle ]
    Assert.Equal<ArticleRecord>([ testArticle ], result)

[<Fact>]
let ``mergeConsecutiveArticles for articles with different feed names`` () =
    let a =
        { testArticle with
            Date = Some(daysSinceEpoch 1)
            FeedName = "FeedA" }

    let b =
        { testArticle with
            Date = Some(daysSinceEpoch 2)
            FeedName = "FeedB" }

    let result = ListArticles.mergeConsecutiveArticles (Posix.now ()) [ a; b ]

    Assert.Equal(2, result.Length)

[<Fact>]
let ``mergeConsecutiveArticles for articles with different titles`` () =
    let a =
        { testArticle with
            Date = Some(daysSinceEpoch 1)
            Title = Some "Title A" }

    let b =
        { testArticle with
            Date = Some(daysSinceEpoch 2)
            Title = Some "Title B" }

    let result = ListArticles.mergeConsecutiveArticles (Posix.now ()) [ a; b ]

    Assert.Equal(2, result.Length)

[<Fact>]
let ``mergeConsecutiveArticles when title is None`` () =
    let a =
        { testArticle with
            Date = Some(daysSinceEpoch 1)
            Content = "content"
            Title = None }

    let b =
        { testArticle with
            Date = Some(daysSinceEpoch 2)
            Content = "content2"
            Title = None }

    let result = ListArticles.mergeConsecutiveArticles (Posix.now ()) [ a; b ]

    Assert.Equal(2, result.Length)
