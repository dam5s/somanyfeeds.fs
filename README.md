# SoManyFeeds

## Local Setup

### Dependencies
 1. .NET 8

### Setup your environment variables.

The file `.env.example` (or `.env.ps1.example` if you use PowerShell) contains the list of 
necessary variables for the whole build.

 * `ENABLE_EXCEPTION_PAGE` is used by Giraffe when running `Damo.Io.Server` locally.

### Running the build

```
make check
make damo.io.server.container
make damo.io.blog.container
```

## GCloud

### Initial container image setup

* Create Artifact Registry in cloud console, named `docker-images`
* Push image to registry, using Powershell:
    ```
    $env:REGION="us-central1"
    $env:PROJECT_ID="example-project-id-1000"
    $env:IMAGE_NAME="damo.io.server"
  
    gcloud auth configure-docker "${env:REGION}-docker.pkg.dev"
    docker tag ${env:IMAGE_NAME} "${env:REGION}-docker.pkg.dev/${env:PROJECT_ID}/docker-images/${env:IMAGE_NAME}:latest"
    docker push "${env:REGION}-docker.pkg.dev/${env:PROJECT_ID}/docker-images/${env:IMAGE_NAME}:latest"
    ```

### Initial cloud run setup

* In Cloud console > Cloud Run > Create Service
    * Select the image we just pushed to our Artifact Registry
    * Min: 0, Max: 1
    * Ingress: All
    * Authentication: Allow Unauthenticated
    * "Create"

### Setting up service account and automation

* Create service account `github-actions-account`
    * Role `Artifact Registry Writer`
    * Role `Cloud Run Developer`
    * Role `Service Account User`
* Create and download key, set as GitHub secret as a single line string
* Setup workflow as seen in `.github/workflows/build.yml`
