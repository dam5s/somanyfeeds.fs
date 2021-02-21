[<RequireQualifiedAccess>]
module SoManyFeedsServer.Admin.UsersPage

open Fable.React
open Fable.React.Props
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open SoManyFeedsPersistence.UsersDataGateway
open SoManyFeedsServer

let private userView (user: UserRecord) =
    div [ Class "card" ]
        [ dl []
            [ dt [] [ str user.Name ]
              dd [] [ str user.Email ]
            ]
        ]

let private view (users: UserRecord seq) =
    let listTitle = h3 [] [ str "All users" ]
    let usersView =
        users
        |> Seq.map userView
        |> Seq.toList

    [ header [ Class "page" ]
        [ div [ Class "page-content" ]
            [ h2 [] [ str "Admin" ]
              h1 [] [ str "Users" ]
            ]
        ]
      div [ Class "main" ]
        [ section []
            [ div [ Class "card-list" ] (listTitle :: usersView)
            ]
        ]
    ]

let page (listUsers: AsyncResult<UserRecord seq>) _ : HttpHandler =
    fun next ctx ->
        task {
            match! listUsers with
            | Ok records ->
                let page = Layout.withoutTabs (view records)
                return! page next ctx

            | Error explanation ->
                return! ErrorPage.page explanation next ctx
        }
