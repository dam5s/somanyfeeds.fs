# SoManyFeeds

## Local Setup

### Dependencies
 1. .NET Core 3.1 (+ maybe Mono for Type Providers on Linux)
 1. PostgreSQL 11

### Database setup
 1. Setup a local Postgresql database.
 1. Create a database user `somanyfeeds` with password `secret`
 1. Create 3 local databases`somanyfeeds_dev`, `somanyfeeds_test`, `somanyfeeds_integration_tests` that belong to user `somanyfeeds`
 1. Migrate the databases `dotnet run -p build db:somanyfeeds:local:migrate`

### Setup your environment variables.

The file `.env.example` contains the list of necessary variables for the whole build.

 * `TWITTER_CONSUMER_API_KEY` is used by `damo-io-server` at runtime only.
 * `TWITTER_CONSUMER_SECRET`  is used by `damo-io-server` at runtime only.
 * `COOKIE_ENCRYPTION_KEY` is used by `somanyfeeds-server` for encryption of JWT cookies
 * `DB_CONNECTION` is set to `Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev`
 * `ENABLE_EXCEPTION_PAGE` is used by Giraffe when running `somanyfeeds-server` locally.

### Running the build

```
./build-all
```
