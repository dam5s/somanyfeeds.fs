FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY . .

RUN dotnet restore

WORKDIR /workspace/Damo.Io.Blog

RUN dotnet run

FROM nginx

COPY --from=build /workspace/Damo.Io.Blog/build/public /usr/share/nginx/html
