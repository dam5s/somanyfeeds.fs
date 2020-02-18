module FableFrontend.ManageApp

open Elmish
open Browser
open Browser.Types
open Fable.SimpleHttp
open FableFrontend.Dialog
open FableFrontend.Feed
open FableFrontend.HttpSupport


type Flags =
    { userName: string
      maxFeeds: int
      feeds: Feed.Json array
      page: string
      searchText: string option }

type Page =
    | List
    | Search

type Model =
    { UserName: string
      MaxFeeds: int
      Feeds: Feed list
      Form: Feed.Fields
      CreationInProgress: bool
      DeleteDialog: Dialog<Feed>
      DeletionInProgress: bool }

type Msg =
    | UpdateFormName of string
    | UpdateFormUrl of string
    | CreateFeed
    | CreateFeedResult of Result<Feed, RequestError>
    | OpenDeleteDialog of Feed
    | CloseDeleteDialog
    | EscapePressed
    | DeleteFeed
    | DeleteFeedResult of Feed * Result<unit, RequestError>

let init (flags: Flags) =
    let feeds = flags.feeds
                |> Array.toList
                |> List.map Feed.fromJson
                |> List.sortBy (fun f -> -f.Id)

    { UserName = flags.userName
      MaxFeeds = flags.maxFeeds
      Feeds = feeds
      Form = Feed.emptyFields
      CreationInProgress = false
      DeleteDialog = Initial
      DeletionInProgress = false }, Cmd.none

let private sendCreateRequest form =
    async {
        let! response = form
                        |> Feed.createRequest
                        |> Http.send

        return if response.statusCode <> 201
               then Error ApiError
               else response
                    |> HttpResponse.parse<Feed.Json>
                    |> Result.map Feed.fromJson
    }

let private sendDeleteRequest feed =
    async {
        let! response = feed
                        |> Feed.deleteRequest
                        |> Http.send

        return if response.statusCode <> 204
               then Error ApiError
               else Ok()
    }

let private feedCreated model newFeed =
    { model with
          CreationInProgress = false
          Form = Feed.emptyFields
          Feeds = [ newFeed ] @ model.Feeds }

let private startDeleteFeed model feed =
    { model with DeletionInProgress = true }, Cmd.ofRequest sendDeleteRequest feed (curry DeleteFeedResult feed)

let private removeFeed model feed =
    { model with
          DeletionInProgress = false
          DeleteDialog = Closed
          Feeds = model.Feeds |> List.filter (fun f -> f.Id <> feed.Id) }

let update msg model =
    let form = model.Form
    let updateForm f = { model with Form = f }

    match msg with
    | UpdateFormName value ->
        updateForm { form with Name = value }, Cmd.none

    | UpdateFormUrl value ->
        updateForm { form with Url = value }, Cmd.none

    | CreateFeed ->
        { model with CreationInProgress = true }, Cmd.ofRequest sendCreateRequest form CreateFeedResult

    | CreateFeedResult (Ok newFeed) ->
        feedCreated model newFeed, Cmd.none

    | CreateFeedResult (Error _) ->
        { model with CreationInProgress = false }, Cmd.none

    | OpenDeleteDialog feed ->
        { model with DeleteDialog = Opened feed }, Cmd.none

    | CloseDeleteDialog ->
        { model with DeleteDialog = Closed }, Cmd.none

    | EscapePressed ->
        model, Cmd.none

    | DeleteFeed ->
        model.DeleteDialog
        |> Dialog.map (startDeleteFeed model)
        |> Dialog.defaultValue (model, Cmd.none)

    | DeleteFeedResult (feed, Ok _) ->
        removeFeed model feed, Cmd.none

    | DeleteFeedResult (_, Error _) ->
        { model with DeletionInProgress = false }, Cmd.none

open Fable.React
open Fable.React.Props

let private overlay model dispatch =
    let onClick msg = OnClick(fun _ -> dispatch msg)

    match model.DeleteDialog with
    | Initial -> div [] []
    | Opened _ -> div [ Class "overlay"; onClick CloseDeleteDialog ] []
    | Closed -> div [ Class "overlay closed" ] []

let private deleteDialog model dispatch =
    let onClick msg = OnClick(fun _ -> dispatch msg)

    match model.DeleteDialog with
    | Opened feed ->
        div [ Class "dialog" ]
            [ h3 [] [ str "Unsubscribe" ]
              p [] [ str(sprintf "Are you sure you want to unsubscribe from \"%s\"?" feed.Name) ]
              nav []
                  [ button
                      [ Class "button primary"
                        Disabled model.DeletionInProgress
                        onClick DeleteFeed ] [ str "Yes, unsubscribe" ]
                    button
                        [ Class "button secondary"
                          Disabled model.DeletionInProgress
                          onClick CloseDeleteDialog ] [ str "No, cancel" ] ] ]
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
                      str "You will need to unsubscribe a feed before you can create a new one." ] ] ]

let private newFeedForm model dispatch =
    let onSubmit msg =
        OnSubmit(fun event ->
            event.preventDefault()
            dispatch msg)

    let onChange msg = OnChange(fun event -> dispatch (msg event.Value))

    section []
        [ form
            [ Class "card"
              onSubmit CreateFeed ]
              [ h3 [] [ str "Add a feed" ]
                label []
                    [ str "Name"
                      input
                          [ Placeholder "Le Monde"
                            Type "text"
                            Name "name"
                            Value model.Form.Name
                            onChange UpdateFormName
                            Disabled model.CreationInProgress ] ]
                label []
                    [ str "Url"
                      input
                          [ Placeholder "https://www.lemonde.fr/rss/une.xml"
                            Type "text"
                            Name "url"
                            Value model.Form.Url
                            onChange UpdateFormUrl
                            Disabled model.CreationInProgress ] ]
                nav []
                    [ button
                        [ Class "button primary"
                          Disabled model.CreationInProgress ] [ str "Subscribe" ] ] ] ]

let private formView model dispatch =
    if hasReachedMaxFeeds model then maxFeedsView
    else newFeedForm model dispatch

let private noFeedsView =
    [ h3 [] [ str "Your feeds" ]
      p [ Class "message" ] [ str "You have not subscribed to any feeds yet." ] ]

let private feedView dispatch feed =
    let onClick msg = OnClick(fun _ -> dispatch msg)

    div [ Class "card" ]
        [ dl []
              [ dt [] [ str "Name" ]
                dd [] [ str feed.Name ]
              ]
          dl []
              [ dt [] [ str "Url" ]
                dd [] [ a [ Href feed.Url; Target "_blank" ] [ str feed.Url ] ]
              ]
          dl []
              [ dt [] []
                dd [ Class "actions" ]
                    [ button [ Class "button secondary"; onClick (OpenDeleteDialog feed) ] [ str "Unsubscribe" ]
                    ]
              ]
        ]

let private feedList model dispatch  =
    let feedsView =
        if List.isEmpty model.Feeds
            then noFeedsView
            else [ h3 [] [ str "Your feeds" ] ] @ (List.map (feedView dispatch) model.Feeds)

    section [] [ div [ Class "card-list" ] feedsView ]

let view model dispatch =
    div []
        [ header [ Class "app-header" ]
              [ div []
                    [ Logo.view
                      nav []
                          [ a [ Href "/" ] [ str "Home" ]
                            a [ Href "/read" ] [ str "Read" ]
                            a
                                [ Href "/manage"
                                  Class "current" ] [ str "Manage" ] ] ] ]
          header [ Class "page" ]
              [ div [ Class "page-content" ]
                    [ h2 [] [ str "Feeds" ]
                      h1 [] [ str "Your subscriptions" ] ] ]
          div [ Class "main" ]
              [ overlay model dispatch
                deleteDialog model dispatch
                formView model dispatch
                feedList model dispatch ] ]

let private onKeyUp dispatch (event: KeyboardEvent) =
    match event.key with
    | "Escape"
    | "Esc" -> dispatch EscapePressed
    | _ ->
        match event.keyCode with
        | 27.0 -> dispatch EscapePressed
        | _ ->
            ()

let subscriptions _ =
    let sub dispatch =
        window.onkeyup <- onKeyUp dispatch

    Cmd.ofSub sub
