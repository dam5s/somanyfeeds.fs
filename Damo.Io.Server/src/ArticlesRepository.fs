[<RequireQualifiedAccess>]
module DamoIoServer.ArticlesRepository

open DamoIoServer.Source
open DamoIoServer.Article

let private aboutContent =
    """
    <p>
      I'm <strong>Damien Le Berrigaud</strong>, Lead Developer at
      <a href="https://initialcapacity.io">initialCapacity[]</a>.

      I like working with
      <a href="https://kotlinlang.org">Kotlin</a>,
      <a href="https://fsharp.org">F#</a>,
      <a href="http://elm-lang.org">Elm</a>,
      <a href="https://developer.android.com">Android</a>,
      <a href="https://developer.apple.com/ios">iOS (Swift or Objective-C)</a>...

      I also sometimes enjoy
      <a href="https://typescriptlang.org">Typescript</a>,
      <a href="https://python.org">Python</a>,
      <a href="https://golang.org">Go</a>...

      I've had previous experiences in
      <a href="https://www.ruby-lang.org">Ruby</a> and
      <a href="https://www.microsoft.com/net">.NET</a>.
    </p>
    <p>
      That's a lot of technologies that I got to work with while working at <em>Pivotal Labs</em>
      and in my previous jobs.
      In order to keep up-to-date, I keep rebuilding this website.
    </p>
    <p>
      This version is written in <em>F#</em> for the frontend and for the backend running on the <em>.NET</em> platform.
      The source code is entirely available <a href="https://github.com/dam5s/somanyfeeds.fs">on my github</a>.
    </p>
    <p>
      You can choose to follow multiple of my feeds, on a single page, by using the menu at the top of the page.
    </p>
    """

let private about: Article =
    { Title = Some "About"
      Link = None
      Content = aboutContent
      Date = None
      Source = About }
    
let mutable private allRecords: Article list = []

let findAll() =
    about :: allRecords

let findAllBySources (sources: Source list) =
    findAll()
    |> List.filter (fun r -> List.contains r.Source sources)

let updateAll newRecords =
    allRecords <- newRecords