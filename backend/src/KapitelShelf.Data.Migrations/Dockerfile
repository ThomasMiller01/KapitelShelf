# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ./KapitelShelf.Data ./KapitelShelf.Data
COPY ./KapitelShelf.Data.Migrations ./KapitelShelf.Data.Migrations
COPY .editorconfig .editorconfig

WORKDIR /src/KapitelShelf.Data.Migrations

RUN dotnet restore "./KapitelShelf.Data.Migrations.csproj" && \
    dotnet build "./KapitelShelf.Data.Migrations.csproj" -c Release -o /app/build && \
    dotnet publish "./KapitelShelf.Data.Migrations.csproj" -c Release -o /app/publish

# production stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "KapitelShelf.Data.Migrations.dll"]
