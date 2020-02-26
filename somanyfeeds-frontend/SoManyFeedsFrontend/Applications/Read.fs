module SoManyFeedsFrontend.Applications.Read

open Elmish
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open SoManyFeedsFrontend.Components
open SoManyFeedsFrontend.Components.Article
open SoManyFeedsFrontend.Components.Feed
open SoManyFeedsFrontend.Components.Logo
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Support.RemoteData
open Time

type Flags =
    { userName: string
      recents: Article.Json array
      feeds: Feed.Json array
      page: string
      selectedFeedId: int64 option }

type Page =
    | Recent of Feed option
    | Bookmarks

type Model =
    { UserName: string
      Recents: RemoteData<Article list>
      Bookmarks: RemoteData<Article list>
      Feeds: Feed list
      Page: Page
      DropdownOpen: bool }

type Msg =
    | EscapePressed
    | NavigateTo of path:string
    | NavigateToAndPushState of path:string
    | ToggleDropdown
    | CloseDropdown
    | ReceivedRecents of Result<Article list, RequestError>
    | ReceivedBookmarks of Result<Article list, RequestError>
    | Bookmark of Article
    | UndoBookmark of Article
    | RemoveBookmark of Article
    | Read of Article
    | Unread of Article
    | ChangeArticleStateResult of Article * ArticleState * Result<unit, RequestError>
    | BookmarkRemoved of Article * Result<unit, RequestError>

let private pageFromFlags flags =
    match flags.page with
    | "Recent" ->
        flags.feeds
        |> Array.filter (fun (f: Feed.Json) -> Some f.id = flags.selectedFeedId)
        |> Array.tryHead
        |> Option.map Feed.fromJson
        |> Recent
    | _ -> Bookmarks

let private (|RecentFeed|_|) (path: string) =
    let prefix = "/read/recent/feed/"

    if path.StartsWith prefix then
        match System.Int64.TryParse(path.Replace(prefix, "")) with
        | true, feedId -> Some feedId
        | false, _ -> None
    else
        None

let private tryFindFeed model feedId =
    model.Feeds |> List.tryFind (fun f -> feedId = f.Id)

let private pageFromPath (model: Model) (path: string) =
    match path with
    | RecentFeed feedId -> Some (Recent(tryFindFeed model feedId))
    | "/read/recent" -> Some (Recent None)
    | "/read/bookmarks" -> Some Bookmarks
    | _ -> None

let private loadArticles request msg =
    let load = fun _ -> async {
        let! response = Http.send request

        return if response.statusCode <> 200
               then Error ApiError
               else
                   response |> HttpResponse.parse (Json.list Article.decoder)
    }
    Cmd.ofRequest load () msg

let private loadBookmarks =
    loadArticles Article.listBookmarksRequest ReceivedBookmarks

let private loadAllFeeds =
    loadArticles Article.listAllRequest ReceivedRecents

let private loadOneFeed feed =
    loadArticles (Article.listByFeedRequest feed) ReceivedRecents

let init flags =
    let page = pageFromFlags flags

    let (bookmarks, cmd) =
        match page with
        | Bookmarks -> Loading, loadBookmarks
        | _ -> NotLoaded, Cmd.none

    { UserName = flags.userName
      Recents =
          flags.recents
          |> Array.toList
          |> List.map Article.fromJson
          |> Loaded
      Bookmarks = bookmarks
      Feeds =
          flags.feeds
          |> Array.toList
          |> List.map Feed.fromJson
      Page = page
      DropdownOpen = false }, cmd

let private pageTitle page =
    match page with
    | Recent None -> "Recent"
    | Recent(Some feed) -> feed.Name
    | Bookmarks -> "Bookmarks"

let private pagePath page =
    match page with
    | Recent None -> "/read/recent"
    | Recent(Some feed) -> sprintf "/read/recent/feed/%d" feed.Id
    | Bookmarks -> "/read/bookmarks"

let menuOptions model (dispatch: Html.Dispatcher<Msg>) =
    let feedPage feed = Recent(Some feed)

    let pages =
        [ Recent None
          Bookmarks ]
        @ (List.map feedPage model.Feeds)

    let pageLink page =
        let path = pagePath page
        let title = pageTitle page
        let msg = NavigateToAndPushState path
        a [ Href path ; dispatch.OnClickPreventingDefault msg ] [ str title ]

    pages
    |> List.filter (fun p -> p <> model.Page)
    |> List.map pageLink

let private cardWithMessages messages =
    let paragraph msg = p [ Class "message" ] [ str msg ]
    section [] [ div [ Class "card" ] (List.map paragraph messages) ]

let private articleView model (dispatch: Html.Dispatcher<Msg>) (record: Article) =
    let articleTitleLink _ =
        let link =
            a
                [ Href record.Url
                  Target "_blank"
                  Html.rawInnerHtml record.Title ] []
        h3 [] [ link ]

    let fullHeader _ =
        header []
            [ h4 [] [ str record.FeedName ]
              articleTitleLink() ]

    let articleDate _ = p [ Class "date" ] [ str (Posix.toString record.Date) ]

    let articleContent _ =
        div
            [ Class "content"
              Html.rawInnerHtml record.Content ] []

    match (model.Page, record.State) with
    | Bookmarks, _ ->
        article [ Class "card" ]
            [ div [ Class "row" ]
                  [ fullHeader()
                    button
                        [ dispatch.OnClick(RemoveBookmark record)
                          Type "button"
                          Class "flex-init button icon-only bookmarked" ] [ str "Mark read" ] ]
              articleDate()
              articleContent() ]
    | _, Article.Unread ->
        article [ Class "card" ]
            [ div [ Class "row" ]
                  [ fullHeader()
                    button
                        [ dispatch.OnClick(Bookmark record)
                          Type "button"
                          Class "flex-init button icon-only bookmark" ] [ str "Save for later" ]
                    button
                        [ dispatch.OnClick(Read record)
                          Type "button"
                          Class "flex-init button icon-only mark-read" ] [ str "Mark read" ] ]
              articleDate()
              articleContent() ]
    | _, Article.Read ->
        article [ Class "card row read" ]
            [ h3 [ Html.rawInnerHtml record.Title ] []
              button
                  [ dispatch.OnClick(Unread record)
                    Type "button"
                    Class "button icon-only undo flex-init" ] [ str "Undo" ] ]
    | _, Article.Bookmarked ->
        article [ Class "card row bookmarked" ]
            [ articleTitleLink()
              button
                  [ dispatch.OnClick(UndoBookmark record)
                    Type "button"
                    Class "button icon-only bookmarked flex-init" ] [ str "Undo bookmark" ] ]

let private recentArticleList model dispatch =
    match model.Recents with
    | NotLoaded -> section [] []

    | Loaded [] ->
        if List.isEmpty model.Feeds then
            section []
                [ div [ Class "card" ]
                      [ p [ Class "message" ]
                            [ str "You have not "
                              Html.link "/manage" "subscribed to any feed"
                              str " yet." ]
                        p [ Class "message" ]
                            [ str "Please use the "
                              Html.link "/manage" "manage tab"
                              str " to subscribe to some feeds." ] ] ]
        else
            cardWithMessages
                [ "No unread articles."; "New feed subscriptions may take ~10 minutes before being available." ]

    | Loaded articles -> section [] (List.map (articleView model dispatch) articles)

    | Loading -> cardWithMessages [ "Loading your reading list. Thank you for your patience." ]

    | RemoteError message -> cardWithMessages [ "There was an error while loading your articles."; message ]

let private bookmarksList model dispatch =
    match model.Bookmarks with
    | NotLoaded -> cardWithMessages [ "Bookmarks not loaded." ]
    | Loaded [] -> cardWithMessages [ "You don't have any bookmarks yet." ]
    | Loaded articles -> section [] (List.map (articleView model dispatch) articles)
    | Loading -> cardWithMessages [ "Loading your bookmarks. Thank you for your patience." ]
    | RemoteError message -> cardWithMessages [ "There was an error while loading your bookmarks."; message ]

let view model d =
    let dispatch = Html.Dispatcher(d)

    let dropdownClass =
        if model.DropdownOpen then "open" else "closed"

    div []
        [ header [ Class "app-header" ]
              [ div []
                    [ div [ Class "page-content" ]
                          [ Logo.view
                            Tabs.view Tabs.Read ] ] ]
          header [ Class "page" ]
              [ div [ Class "row align-end responsive" ]
                    [ div []
                          [ h2 [] [ str "Articles" ]
                            h1 [] [ str (pageTitle model.Page) ] ]
                      nav [ Class "flex-init" ]
                          [ div
                              [ Class("toggle " + dropdownClass)
                                dispatch.OnClick ToggleDropdown ] [ str "Filters" ]
                            menu [ Class dropdownClass ] (menuOptions model dispatch) ] ] ]
          div [ Class "main" ]
              [ match model.Page with
                | Recent _ -> recentArticleList model dispatch
                | Bookmarks -> bookmarksList model dispatch ] ]

let private doSimpleRequest request =
    async {
        let! response = Http.send request

        return if response.statusCode <> 200 then Error ApiError else Ok()
    }

let private createBookmark = Article.createBookmarkRequest >> doSimpleRequest
let private deleteBookmark = Article.deleteBookmarkRequest >> doSimpleRequest
let private createReadArticle = Article.createReadArticleRequest >> doSimpleRequest
let private deleteReadArticle = Article.deleteReadArticleRequest >> doSimpleRequest

let private changePage model (withPush: bool) newPath =
    match pageFromPath model newPath with
    | Some newPage ->
        let newPageModel = { model with DropdownOpen = false; Page = newPage }
        let batch cmd =
            if withPush then
                Cmd.batch [ Navigation.pushPath newPath ; cmd ]
            else
                cmd

        match newPage with
        | Recent None ->
            { newPageModel with Recents = Loading },
            batch loadAllFeeds
        | Recent (Some feed) ->
            { newPageModel with Recents = Loading },
            batch (loadOneFeed feed)
        | Bookmarks ->
            { newPageModel with Bookmarks = Loading },
            batch loadBookmarks
    | None ->
        model, Navigation.goTo newPath

let update msg model =
    match msg with
    | EscapePressed ->
        { model with DropdownOpen = false },
        Cmd.none
    | NavigateTo newPath ->
        changePage model false newPath
    | NavigateToAndPushState newPath ->
        changePage model true newPath
    | ToggleDropdown ->
        { model with DropdownOpen = not model.DropdownOpen },
         Cmd.none
    | CloseDropdown ->
        { model with DropdownOpen = false },
         Cmd.none
    | ReceivedRecents(Ok articles) ->
        { model with Recents = Loaded articles },
         Cmd.none
    | ReceivedRecents(Error err) ->
        { model with Recents = RemoteError(RequestError.userMessage err) },
         Cmd.none
    | ReceivedBookmarks(Ok articles) ->
        { model with Bookmarks = Loaded articles },
         Cmd.none
    | ReceivedBookmarks(Error err) ->
        { model with Bookmarks = RemoteError(RequestError.userMessage err) },
         Cmd.none
    | Bookmark record ->
        model,
         Cmd.ofRequest createBookmark record (curry2 ChangeArticleStateResult record Article.Bookmarked)
    | UndoBookmark record ->
        model,
         Cmd.ofRequest deleteBookmark record (curry2 ChangeArticleStateResult record Article.Unread)
    | RemoveBookmark record ->
        model,
         Cmd.ofRequest deleteBookmark record (curry BookmarkRemoved record)
    | Read record ->
        model,
         Cmd.ofRequest createReadArticle record (curry2 ChangeArticleStateResult record Article.Read)
    | Unread record ->
        model,
         Cmd.ofRequest deleteReadArticle record (curry2 ChangeArticleStateResult record Article.Unread)
    | ChangeArticleStateResult(record, newState, Ok _) ->
        { model with Recents = RemoteData.map (Article.setState newState record) model.Recents },
         Cmd.none
    | ChangeArticleStateResult(_, _, Error _) ->
        model,
        Cmd.none
    | BookmarkRemoved(record, Ok _) ->
        { model with Bookmarks = RemoteData.map (List.filter (fun r -> r <> record)) model.Bookmarks },
        Cmd.none
    | BookmarkRemoved(_, Error _) ->
        model,
        Cmd.none

open Browser

let subscriptions _ =
    let sub dispatch =
        window.onkeyup <- Keyboard.onEscape EscapePressed dispatch
        window.onpopstate <- Navigation.onPathChanged NavigateTo dispatch

    Cmd.ofSub sub
