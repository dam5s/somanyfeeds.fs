FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY . .

RUN dotnet restore

WORKDIR /workspace/Damo.Io.Server

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY --from=build /workspace/Damo.Io.Server/out .

CMD ["dotnet", "Damo.Io.Server.dll"]
