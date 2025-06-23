# build stage for Migrations
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-migrations

WORKDIR /src

COPY ./KapitelShelf.Data ./KapitelShelf.Data
COPY ./KapitelShelf.Data.Migrations ./KapitelShelf.Data.Migrations
COPY .editorconfig .editorconfig

WORKDIR /src/KapitelShelf.Data.Migrations

RUN dotnet restore "./KapitelShelf.Data.Migrations.csproj" && \
    dotnet build "./KapitelShelf.Data.Migrations.csproj" -c Release -o /app/build && \
    dotnet publish "./KapitelShelf.Data.Migrations.csproj" -c Release -o /app/publish

# build stage for API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-api

WORKDIR /src

COPY ./KapitelShelf.Api ./KapitelShelf.Api
COPY ./KapitelShelf.Data ./KapitelShelf.Data
COPY .editorconfig .editorconfig

WORKDIR /src/KapitelShelf.Api

RUN dotnet restore "./KapitelShelf.Api.csproj" && \
    dotnet build "./KapitelShelf.Api.csproj" -c Release -o /app/build && \
    dotnet publish "./KapitelShelf.Api.csproj" -c Release -o /app/publish

# production stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production

WORKDIR /app

# install curl for docker-compose healthchecks
RUN apt-get update && apt-get install -y curl

COPY --from=build-api /app/publish ./api
COPY --from=build-migrations /app/publish ./migrations

EXPOSE 5261
ENV HTTP_PORTS=5261

ENTRYPOINT ["sh", "-c", "cd ./migrations && dotnet KapitelShelf.Data.Migrations.dll && cd ../api && dotnet KapitelShelf.Api.dll"]
