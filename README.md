# SoManyFeeds

## Local Setup

### Dependencies
 1. .NET Core 3.1 (+ maybe Mono for Type Providers on Linux)
 1. PostgreSQL 11

### Database setup
 1. Setup a local Postgresql database.
 1. Create a database user `somanyfeeds` with password `secret`
 1. Create a database called `somanyfeeds_dev` that belongs to `somanyfeeds`
 1. Create a database called `somanyfeeds_test` that belongs to `somanyfeeds`
 1. Migrate the database `dotnet run -p build db:somanyfeeds:local:migrate`

### Chrome + Chrome driver.

For integration tests you'll need a version of Chrome and its matching Chrome driver.
I recommend putting the driver in the same folder as Chrome and its binary.
Then set the `CHROME_DRIVER_DIR` environment variable in your `.env` file. 

### Setup your environment variables.

The file `.env.example` contains the list of necessary variables for the whole build.

 * `TWITTER_CONSUMER_API_KEY` is used by `damo-io-server` at runtime only.
 * `TWITTER_CONSUMER_SECRET`  is used by `damo-io-server` at runtime only.
 * `COOKIE_ENCRYPTION_KEY` is used by `somanyfeeds-server` for encryption of JWT cookies
 * `DB_CONNECTION` is set to `Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev`
 * `CHROME_DRIVER_DIR` is necessary for the integration tests, e.g. `/home/dam5s/dev/chromium`
 * `ENABLE_EXCEPTION_PAGE` is used by Giraffe when running `somanyfeeds-server` locally.

### Running the build

```
./build-all
```
