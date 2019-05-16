module SoManyFeeds.Read exposing (main)

import Browser exposing (Document, UrlRequest)
import Browser.Navigation as Nav exposing (Key)
import Html exposing (Attribute, Html, a, article, button, div, h1, h2, h3, h4, header, menu, nav, p, section, text)
import Html.Attributes exposing (class, href, target, type_)
import Html.Events exposing (onClick)
import Http
import Keyboard exposing (RawKey)
import List.Extra
import SoManyFeeds.Components.Article as Article exposing (Article)
import SoManyFeeds.Components.Feed exposing (Feed)
import SoManyFeeds.Components.Logo as Logo
import SoManyFeeds.Support.Keyboard as Keyboard
import SoManyFeeds.Support.RemoteData as RemoteData exposing (RemoteData(..))
import Support.DateFormat as DateFormat
import Support.RawHtml as RawHtml
import Task
import Time
import Url exposing (Url)
import Url.Parser as Parser exposing ((</>), Parser)


type alias Flags =
    { userName : String
    , recents : List Article.Json
    , feeds : List Feed
    , page : String
    , selectedFeedId : Maybe Int
    }


type Page
    = Recent (Maybe Feed)
    | Bookmarks


urlParser : Model -> Parser (Page -> a) a
urlParser model =
    let
        recentFeedPage id =
            model.feeds
                |> List.filter (\f -> f.id == id)
                |> List.head
                |> Recent
    in
    Parser.oneOf
        [ Parser.map (Recent Nothing) (Parser.s "read" </> Parser.s "recent")
        , Parser.map Bookmarks (Parser.s "read" </> Parser.s "bookmarks")
        , Parser.map recentFeedPage (Parser.s "read" </> Parser.s "recent" </> Parser.s "feed" </> Parser.int)
        ]


pageFromUrl : Model -> Url.Url -> Maybe Page
pageFromUrl model url =
    Parser.parse (urlParser model) url


type alias Model =
    { navKey : Key
    , userName : String
    , recents : RemoteData (List Article)
    , bookmarks : RemoteData (List Article)
    , feeds : List Feed
    , page : Page
    , dropdownOpen : Bool
    , timeZone : Maybe Time.Zone
    }


type Msg
    = ClickedLink UrlRequest
    | ChangedUrl Url
    | KeyWasPressed RawKey
    | ToggleDropdown
    | CloseDropdown
    | ReceivedRecents (Result Http.Error (List Article.Json))
    | ReceivedBookmarks (Result Http.Error (List Article.Json))
    | UpdateTimeZone Time.Zone
    | Bookmark Article
    | UndoBookmark Article
    | RemoveBookmark Article
    | Read Article
    | Unread Article
    | ChangeArticleStateResult Article Article.State (Result Http.Error String)
    | BookmarkRemoved Article (Result Http.Error String)


pageFromFlags : Flags -> Page
pageFromFlags flags =
    case flags.page of
        "Recent" ->
            flags.feeds
                |> List.filter (\f -> Just f.id == flags.selectedFeedId)
                |> List.head
                |> Recent

        _ ->
            Bookmarks


init : Flags -> Url -> Key -> ( Model, Cmd Msg )
init flags _ navKey =
    let
        page =
            pageFromFlags flags

        loadTimeZone =
            Task.perform UpdateTimeZone Time.here

        ( bookmarks, cmds ) =
            case page of
                Bookmarks ->
                    ( Loading, Cmd.batch [ loadTimeZone, loadBookmarks ] )

                _ ->
                    ( NotLoaded, loadTimeZone )
    in
    ( { navKey = navKey
      , userName = flags.userName
      , recents = Loaded (List.map Article.fromJson flags.recents)
      , bookmarks = bookmarks
      , feeds = flags.feeds
      , page = page
      , dropdownOpen = False
      , timeZone = Nothing
      }
    , cmds
    )


articleView : Model -> Article -> Html Msg
articleView model record =
    case ( model.page, record.state ) of
        ( Bookmarks, _ ) ->
            article [ class "card" ]
                [ div [ class "row" ]
                    [ header []
                        [ h4 [] [ text record.feedName ]
                        , h3 [] [ a [ href record.url, target "_blank" ] <| RawHtml.parseEntities record.title ]
                        ]
                    , button [ onClick (RemoveBookmark record), type_ "button", class "flex-init button icon-only bookmarked" ] [ text "Mark read" ]
                    ]
                , p [ class "date" ] [ text <| DateFormat.tryFormat model.timeZone record.date ]
                , div [ class "content" ] <| RawHtml.fromString record.content
                ]

        ( _, Article.Unread ) ->
            article [ class "card" ]
                [ div [ class "row" ]
                    [ header []
                        [ h4 [] [ text record.feedName ]
                        , h3 [] [ a [ href record.url, target "_blank" ] <| RawHtml.parseEntities record.title ]
                        ]
                    , button [ onClick (Bookmark record), type_ "button", class "flex-init button icon-only bookmark" ] [ text "Save for later" ]
                    , button [ onClick (Read record), type_ "button", class "flex-init button icon-only mark-read" ] [ text "Mark read" ]
                    ]
                , p [ class "date" ] [ text <| DateFormat.tryFormat model.timeZone record.date ]
                , div [ class "content" ] <| RawHtml.fromString record.content
                ]

        ( _, Article.Read ) ->
            article [ class "card row read" ]
                [ h3 [] <| RawHtml.parseEntities record.title
                , button [ onClick (Unread record), type_ "button", class "button icon-only undo flex-init" ] [ text "Undo" ]
                ]

        ( _, Article.Bookmarked ) ->
            article [ class "card row bookmarked" ]
                [ h3 [] [ a [ href record.url, target "_blank" ] <| RawHtml.parseEntities record.title ]
                , button [ onClick (UndoBookmark record), type_ "button", class "button icon-only bookmarked flex-init" ] [ text "Undo bookmark" ]
                ]


cardWithMessages : List String -> Html Msg
cardWithMessages messages =
    let
        paragraph message =
            p [ class "message" ] [ text message ]
    in
    section []
        [ div [ class "card" ] (List.map paragraph messages) ]


bookmarksList : Model -> Html Msg
bookmarksList model =
    case model.bookmarks of
        NotLoaded ->
            cardWithMessages [ "Bookmarks not loaded." ]

        Loaded [] ->
            cardWithMessages [ "You don't have any bookmarks yet." ]

        Loaded articles ->
            section [] (List.map (articleView model) articles)

        Loading ->
            cardWithMessages [ "Loading your bookmarks. Thank you for your patience." ]

        Error message ->
            cardWithMessages [ "There was an error while loading your bookmarks.", message ]


recentArticleList : Model -> Html Msg
recentArticleList model =
    case model.recents of
        NotLoaded ->
            section [] []

        Loaded [] ->
            if List.isEmpty model.feeds then
                section []
                    [ div [ class "card" ]
                        [ p [ class "message" ] [ text "You have not ", a [ href "/manage" ] [ text "subscribed to any feed" ], text " yet." ]
                        , p [ class "message" ] [ text "Please use the ", a [ href "/manage" ] [ text "manage tab" ], text " to subscribe to some feeds." ]
                        ]
                    ]

            else
                cardWithMessages [ "No unread articles.", "New feed subscriptions may take ~10 minutes before being available." ]

        Loaded articles ->
            section [] (List.map (articleView model) articles)

        Loading ->
            cardWithMessages [ "Loading your reading list. Thank you for your patience." ]

        Error message ->
            cardWithMessages [ "There was an error while loading your articles.", message ]


pageTitle : Page -> String
pageTitle page =
    case page of
        Recent Nothing ->
            "Recent"

        Recent (Just feed) ->
            feed.name

        Bookmarks ->
            "Bookmarks"


pagePath : Page -> String
pagePath page =
    case page of
        Recent Nothing ->
            "/read/recent"

        Recent (Just feed) ->
            "/read/recent/feed/" ++ String.fromInt feed.id

        Bookmarks ->
            "/read/bookmarks"


menuOptions : Model -> List (Html Msg)
menuOptions model =
    let
        feedPage feed =
            Recent (Just feed)

        pages =
            [ Recent Nothing, Bookmarks ] ++ List.map feedPage model.feeds

        pageLink page =
            a [ href (pagePath page) ] [ text (pageTitle page) ]
    in
    pages
        |> List.filter (\p -> p /= model.page)
        |> List.map pageLink


view : Model -> Document Msg
view model =
    let
        dropdownClass =
            if model.dropdownOpen then
                "open"

            else
                "closed"
    in
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [ class "app-header" ]
            [ div []
                [ div [ class "page-content" ]
                    [ Logo.view
                    , nav []
                        [ a [ href "/" ] [ text "Home" ]
                        , a [ href "/read", class "current" ] [ text "Read" ]
                        , a [ href "/manage" ] [ text "Manage" ]
                        ]
                    ]
                ]
            ]
        , header [ class "page" ]
            [ div [ class "row align-end responsive" ]
                [ div []
                    [ h2 [] [ text "Articles" ]
                    , h1 [] [ text (pageTitle model.page) ]
                    ]
                , nav [ class "flex-init" ]
                    [ div [ class ("toggle " ++ dropdownClass), onClick ToggleDropdown ] [ text "Filters" ]
                    , menu [ class dropdownClass ] (menuOptions model)
                    ]
                ]
            ]
        , div [ class "main" ]
            [ case model.page of
                Recent _ ->
                    recentArticleList model

                Bookmarks ->
                    bookmarksList model
            ]
        ]
    }


loadAllFeeds : Cmd Msg
loadAllFeeds =
    Http.send ReceivedRecents Article.listAllRequest


loadFeed : Feed -> Cmd Msg
loadFeed feed =
    Http.send ReceivedRecents (Article.listByFeedRequest feed)


loadBookmarks : Cmd Msg
loadBookmarks =
    Http.send ReceivedBookmarks Article.listBookmarksRequest


changePage : Model -> Url.Url -> ( Model, Cmd Msg )
changePage model url =
    case pageFromUrl model url of
        Just newPage ->
            let
                newPageModel =
                    { model | dropdownOpen = False, page = newPage }

                pushUrl =
                    Nav.pushUrl model.navKey (Url.toString url)
            in
            case newPage of
                Recent Nothing ->
                    ( { newPageModel | recents = Loading }, Cmd.batch [ pushUrl, loadAllFeeds ] )

                Recent (Just feed) ->
                    ( { newPageModel | recents = Loading }, Cmd.batch [ pushUrl, loadFeed feed ] )

                Bookmarks ->
                    ( { newPageModel | bookmarks = Loading }, Cmd.batch [ pushUrl, loadBookmarks ] )

        Nothing ->
            ( model, Nav.load (Url.toString url) )


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ClickedLink (Browser.Internal url) ->
            changePage model url

        ClickedLink (Browser.External url) ->
            ( model, Nav.load url )

        ChangedUrl _ ->
            ( model, Cmd.none )

        KeyWasPressed k ->
            if Keyboard.isEscape k then
                ( { model | dropdownOpen = False }, Cmd.none )

            else
                ( model, Cmd.none )

        ToggleDropdown ->
            ( { model | dropdownOpen = not model.dropdownOpen }, Cmd.none )

        CloseDropdown ->
            ( { model | dropdownOpen = False }, Cmd.none )

        ReceivedRecents (Ok articles) ->
            ( { model | recents = Loaded (List.map Article.fromJson articles) }, Cmd.none )

        ReceivedRecents (Err err) ->
            ( { model | recents = RemoteData.errorFromHttp err }, Cmd.none )

        ReceivedBookmarks (Ok articles) ->
            ( { model | bookmarks = Loaded (List.map Article.fromJson articles) }, Cmd.none )

        ReceivedBookmarks (Err err) ->
            ( { model | bookmarks = RemoteData.errorFromHttp err }, Cmd.none )

        UpdateTimeZone timeZone ->
            ( { model | timeZone = Just timeZone }, Cmd.none )

        Bookmark record ->
            ( model, Http.send (ChangeArticleStateResult record Article.Bookmarked) (Article.bookmarkRequest record) )

        UndoBookmark record ->
            ( model, Http.send (ChangeArticleStateResult record Article.Unread) (Article.removeBookmarkRequest record) )

        RemoveBookmark record ->
            ( model, Http.send (BookmarkRemoved record) (Article.removeBookmarkRequest record) )

        Read record ->
            ( model, Http.send (ChangeArticleStateResult record Article.Read) (Article.readRequest record) )

        Unread record ->
            ( model, Http.send (ChangeArticleStateResult record Article.Unread) (Article.unreadRequest record) )

        ChangeArticleStateResult record newState (Ok _) ->
            let
                updatedRecents =
                    RemoteData.map (Article.setState newState record) model.recents
            in
            ( { model | recents = updatedRecents }, Cmd.none )

        ChangeArticleStateResult _ _ (Err _) ->
            ( model, Cmd.none )

        BookmarkRemoved record (Ok _) ->
            let
                updatedBookmarks =
                    RemoteData.map (List.Extra.remove record) model.bookmarks
            in
            ( { model | bookmarks = updatedBookmarks }, Cmd.none )

        BookmarkRemoved _ (Err _) ->
            ( model, Cmd.none )


subscriptions : Model -> Sub Msg
subscriptions _ =
    Keyboard.ups KeyWasPressed


main : Program Flags Model Msg
main =
    Browser.application
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        , onUrlRequest = ClickedLink
        , onUrlChange = ChangedUrl
        }
