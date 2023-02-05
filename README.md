# SoManyFeeds

## Local Setup

### Dependencies
 1. .NET 7
 1. Node JS (LTS or latest)

### Setup your environment variables.

The file `.env.example` (or `.env.ps1.example` if you use PowerShell) contains the list of necessary variables for the whole build.

 * `ENABLE_EXCEPTION_PAGE` is used by Giraffe when running `Damo.Io.Server` locally.

### Running the build

With Bash -
```
./build.sh
```

With PowerShell -
```
./build.ps1
```
