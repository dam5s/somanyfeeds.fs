# SoManyFeeds

## Local Setup

### Dependencies
 1. .NET 6
 1. PostgreSQL 11
 1. Node JS

### Database setup
 1. Setup a local Postgresql database.
 1. Create a database user `somanyfeeds` with password `secret`
 1. Create 3 local databases`somanyfeeds_dev`, `somanyfeeds_test`, `somanyfeeds_integration_tests` that belong to user `somanyfeeds`
 1. Migrate the databases `dotnet run -p Build db:somanyfeeds:local:migrate`

```sql
create user somanyfeeds with password 'secret';
create database somanyfeeds_dev;
create database somanyfeeds_tests;
create database somanyfeeds_integration_tests;
grant all privileges on database somanyfeeds_dev to somanyfeeds;
grant all privileges on database somanyfeeds_tests to somanyfeeds;
grant all privileges on database somanyfeeds_integration_tests to somanyfeeds;
```

### Setup your environment variables.

The file `.env.example` contains the list of necessary variables for the whole build.

 * `TWITTER_CONSUMER_API_KEY` is used by `Damo.Io.Server` at runtime only.
 * `TWITTER_CONSUMER_SECRET`  is used by `Damo.Io.Server` at runtime only.
 * `COOKIE_ENCRYPTION_KEY` is used by `SoManyFeeds.Server` for encryption of JWT cookies
 * `DB_CONNECTION` is set to `Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev`
 * `ENABLE_EXCEPTION_PAGE` is used by Giraffe when running `SoManyFeeds.Server` locally.

### Running the build

```
./build-all
```
