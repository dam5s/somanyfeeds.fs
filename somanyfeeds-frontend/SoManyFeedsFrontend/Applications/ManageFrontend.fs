module SoManyFeedsFrontend.Applications.ManageFrontend

open Elmish
open Browser
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Dialog
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Support.RemoteData
open SoManyFeedsFrontend.Components.Feed
open SoManyFeedsFrontend.Components.Search


type Flags =
    { UserName: string
      MaxFeeds: int
      Feeds: Feed.Json array
      Page: string
      SearchText: string }

type Page =
    | List
    | Search

type Model =
    { UserName: string
      MaxFeeds: int
      Feeds: Feed list
      Page: Page
      Form: Search.Form
      DeleteDialog: Dialog<Feed>
      DeletionInProgress: bool }

type Msg =
    | UpdateFormText of string
    | DoSearch
    | SearchResult of Result<SearchResult list, RequestError>
    | OpenDeleteDialog of Feed
    | CloseDeleteDialog
    | EscapePressed
    | NavigateTo of path:string
    | DeleteFeed
    | DeleteFeedResult of Feed * Result<unit, RequestError>
    | Subscribe of SearchResult
    | SubscriptionResult of Result<Feed, RequestError>

let private pageFromFlags (flags: Flags) =
    match flags.Page with
    | "List" -> List
    | _ -> Search

let private (|SearchQuery|_|) (path: string) =
    let prefix = "/manage/search/"

    if path.StartsWith prefix then
        Some (path.Replace(prefix, ""))
    else
        None

let private startSearch query =
    Cmd.ofRequest Search.sendRequest query SearchResult

let private setSearching model query =
    { model with
        Form = { model.Form with Results = Loading; Text = query  }
        Page = Search
    }

let private navigateTo (model: Model) (path: string): Model * Cmd<Msg> =
    match path with
    | "/manage/list" -> { model with Page = List }, Cmd.none
    | "/manage/search" -> { model with Page = Search }, Cmd.none
    | SearchQuery query -> setSearching model query, startSearch query
    | _ -> { model with Page = List }, Cmd.none

let initModel (flags: Flags) =
    let feeds =
        flags.Feeds
        |> Array.toList
        |> List.map Feed.fromJson
        |> List.sortBy (fun f -> -f.Id)

    { UserName = flags.UserName
      MaxFeeds = flags.MaxFeeds
      Feeds = feeds
      Page = pageFromFlags flags
      Form = Search.initForm flags.SearchText
      DeleteDialog = Initial
      DeletionInProgress = false }

let init (flags: Flags) =
    let initCmd =
        match flags.SearchText with
        | "" -> Cmd.none
        | text -> startSearch text

    initModel flags, initCmd

let private startDeleteFeed model feed =
    let cmd = Cmd.ofRequest Feed.sendDeleteRequest feed (curry DeleteFeedResult feed)
    { model with DeletionInProgress = true }, cmd

let private removeFeed model feed =
    { model with
          DeletionInProgress = false
          DeleteDialog = Closed
          Feeds = model.Feeds
                  |> List.filter (fun f -> f.Id <> feed.Id) }

let private updateResultName result newName =
    List.updateOne result (SearchResult.updateName newName)

let private subscribeToFeed searchResult =
    Cmd.ofRequest Feed.sendCreateRequest searchResult SubscriptionResult

let private searchPath (form: Search.Form) =
    match form.Text with
    | "" -> "/manage/search"
    | query -> $"/manage/search/%s{Http.urlEncode query}"

let update msg model =
    let form = model.Form
    let updateForm f = { model with Form = f }

    match msg with
    | UpdateFormText value ->
        updateForm { form with Text = value; Results = NotLoaded }, Cmd.none

    | DoSearch ->
        { updateForm { form with Results = Loading } with Page = Search },
        Cmd.batch [ Navigation.pushPath (searchPath model.Form) ; startSearch form.Text ]

    | SearchResult (Ok results) ->
        updateForm { form with Results = Loaded results }, Cmd.none

    | SearchResult (Error err) ->
        updateForm { form with Results = RemoteError (RequestError.userMessage err) }, Cmd.none

    | OpenDeleteDialog feed ->
        { model with DeleteDialog = Opened feed }, Cmd.none

    | CloseDeleteDialog ->
        { model with DeleteDialog = Closed }, Cmd.none

    | EscapePressed ->
        model, Cmd.none

    | NavigateTo path ->
        navigateTo model path

    | DeleteFeed ->
        model.DeleteDialog
        |> Dialog.map (startDeleteFeed model)
        |> Dialog.defaultValue (model, Cmd.none)

    | DeleteFeedResult (feed, Ok _) ->
        removeFeed model feed, Cmd.none

    | DeleteFeedResult (_, Error _) ->
        { model with DeletionInProgress = false }, Cmd.none

    | Subscribe result ->
        model, subscribeToFeed result

    | SubscriptionResult (Ok newFeed) ->
        { model with Feeds = [ newFeed ] @ model.Feeds }, Cmd.none

    | SubscriptionResult (Error _) ->
        failwith "TODO"

open Fable.React
open Fable.React.Props
open SoManyFeedsFrontend.Components
open SoManyFeedsFrontend.Components.Logo

let private overlay model (dispatch: Html.Dispatcher<Msg>) =
    match model.DeleteDialog with
    | Initial -> div [] []
    | Opened _ -> div [ Class "overlay"; dispatch.OnClick CloseDeleteDialog ] []
    | Closed -> div [ Class "overlay closed" ] []

let private deleteDialog model (dispatch: Html.Dispatcher<Msg>) =
    match model.DeleteDialog with
    | Opened feed ->
        div [ Class "dialog" ]
            [ h3 [] [ str "Unsubscribe" ]
              p [] [ str $"Are you sure you want to unsubscribe from \"%s{feed.Name}\"?" ]
              nav []
                  [ button
                        [ Class "button primary"
                          Disabled model.DeletionInProgress
                          dispatch.OnClick DeleteFeed
                        ] [ str "Yes, unsubscribe" ]
                    button
                        [ Class "button secondary"
                          Disabled model.DeletionInProgress
                          dispatch.OnClick CloseDeleteDialog
                        ] [ str "No, cancel" ]
                  ]
            ]
    | _ ->
        div [] []

let private hasReachedMaxFeeds model =
    List.length model.Feeds >= model.MaxFeeds

let private maxFeedsView =
    section []
        [ div [ Class "card" ]
            [ h3 [] [ str "Feeds limit reached" ]
              p [ Class "message" ]
                  [ str "You have reached your feed subscription limit. "
                    str "You will need to unsubscribe a feed before you can create a new one." ]
            ]
        ]

let private searchFeedForm model (dispatch: Html.Dispatcher<Msg>) =
    let formDisabled =
        match model.Form.Results with
        | NotLoaded | Loaded _ | RemoteError _ -> false
        | Loading -> true

    section []
        [ form
            [ Class "card"; dispatch.OnSubmit DoSearch ]
            [ h3 [] [ str "Add feed" ]
              label []
                  [ str "Url"
                    input
                        [ Placeholder "https://www.lemonde.fr"
                          Type "text"
                          Name "searchText"
                          DefaultValue model.Form.Text
                          dispatch.OnChange UpdateFormText
                          Disabled formDisabled ] ]
              nav []
                  [ button
                      [ Class "button primary"
                        Disabled formDisabled ] [ str "Search" ] ] ] ]

let private formView model (dispatch: Html.Dispatcher<Msg>) =
    if hasReachedMaxFeeds model
        then maxFeedsView
        else searchFeedForm model dispatch

let private noFeedsView =
    [ h3 [] [ str "Your feeds" ]
      p [ Class "message" ] [ str "You have not subscribed to any feeds yet." ] ]

let private feedView (dispatch: Html.Dispatcher<Msg>) (feed: Feed) =
    div [ Class "card" ]
        [ dl []
              [ dt [] [ str feed.Name ]
                dd [] [ Html.extLink feed.Url feed.Url ]
              ]
          div [ Class "actions" ]
              [ button [ Class "button secondary"; dispatch.OnClick (OpenDeleteDialog feed) ] [ str "Unsubscribe" ]
              ]
        ]

let private feedList model (dispatch: Html.Dispatcher<Msg>) =
    let feedsView =
        if List.isEmpty model.Feeds
            then noFeedsView
            else [ h3 [] [ str "Your feeds" ] ] @ (List.map (feedView dispatch) model.Feeds)

    section [] [ div [ Class "card-list" ] feedsView ]

let private searchResultView (dispatch: Html.Dispatcher<Msg>) (result: SearchResult) =
    div [ Class "card" ]
        [ dl []
              [ dt [] [ str result.Name ]
                dd [] [ str result.Description ]
                dd [] [ Html.extLink result.Url result.Url ]
              ]
          div [ Class "actions" ]
              [ button [ Class "button secondary"; dispatch.OnClick (Subscribe result) ] [ str "Subscribe" ]
              ]
        ]

let private cardView message =
    div [ Class "card" ] [ str message ]

let private searchResultsList model (dispatch: Html.Dispatcher<Msg>) =
    let searchResultsTitle = [ h3 [] [ str "Search results" ] ]

    let searchResultsView =
        match model.Form.Results with
        | NotLoaded -> searchResultsTitle
        | Loading -> searchResultsTitle @ [ cardView "Loading results..." ]
        | Loaded [] -> searchResultsTitle @ [ cardView "No results found" ]
        | Loaded results -> searchResultsTitle @ (List.map (searchResultView dispatch) results)
        | RemoteError _ -> searchResultsTitle @ [ cardView "There was an error loading results. Please try again later." ]

    section [] [ div [ Class "card-list" ] searchResultsView ]

let view model d =
    let dispatch = Html.Dispatcher(d)
    let title, pageView =
        match model.Page with
        | Page.List -> "Your subscriptions", feedList model dispatch
        | Page.Search -> "Search", searchResultsList model dispatch

    div []
        [ header [ Class "app-header" ]
              [ div []
                    [ Logo.view
                      Tabs.view Tabs.Manage
                    ]
              ]
          header [ Class "page" ]
              [ div [ Class "page-content" ]
                    [ h2 [] [ str "Feeds" ]
                      h1 [] [ str title ]
                    ]
              ]
          div [ Class "main" ]
              [ overlay model dispatch
                deleteDialog model dispatch
                formView model dispatch
                pageView
              ]
        ]

let subscriptions _ =
    let sub dispatch =
        window.onkeyup <- Keyboard.onEscape EscapePressed dispatch
        window.onpopstate <- Navigation.onPathChanged NavigateTo dispatch

    Cmd.ofSub sub
