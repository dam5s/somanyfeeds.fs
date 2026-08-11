FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /workspace

COPY . .

RUN dotnet restore

WORKDIR /workspace/apps/Damo.Io.Blog

RUN dotnet run

FROM nginx

COPY --from=build /workspace/apps/Damo.Io.Blog/build/public /usr/share/nginx/html
