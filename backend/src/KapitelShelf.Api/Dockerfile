# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

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

COPY --from=build /app/publish .

EXPOSE 5261
ENV HTTP_PORTS=5261

ENTRYPOINT ["dotnet", "KapitelShelf.Api.dll"]
