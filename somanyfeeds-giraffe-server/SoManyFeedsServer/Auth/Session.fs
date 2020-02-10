module SoManyFeedsServer.Auth.Session

open System
open Microsoft.AspNetCore.Http
open SoManyFeeds
open SoManyFeeds.User

let private encryptionKey = Env.requireVar "COOKIE_ENCRYPTION_KEY"
let private sessionCookieName = "session"
let private urlCookieName = "url"

let private cookieOptions =
    let options = CookieOptions()
    options.MaxAge <- Nullable(TimeSpan.FromDays 4.0)
    options.HttpOnly <- true
    options.IsEssential <- true
    options

let saveUrl (ctx: HttpContext) =
    let url = ctx.Request.Path.ToUriComponent()
    ctx.Response.Cookies.Append(urlCookieName, url, cookieOptions)
    ctx

let takeUrl (ctx: HttpContext): string option =
    let url = ctx.Request.Cookies.Item(urlCookieName)
    ctx.Response.Cookies.Delete(urlCookieName)
    Option.ofObj url

let setUser (user: User) (ctx: HttpContext) =
    let payload = Payload.fromUser user
    let token = JWT.encode encryptionKey payload
    ctx.Response.Cookies.Append(sessionCookieName, token, cookieOptions)
    ctx

let getUser (ctx: HttpContext): User option =
    match ctx.Request.Cookies.Item(sessionCookieName) with
    | null -> None
    | token ->
        let payload = JWT.decode encryptionKey token
        Payload.tryGetUser payload

let clear (ctx: HttpContext) =
    ctx.Response.Cookies.Delete(sessionCookieName)
    ctx
