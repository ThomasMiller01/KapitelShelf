# Installation Guide

Installation methods:

- [Helm Chart](#helm-chart)
- [docker-compose](#docker-compose)
- [Docker (Standalone)](#docker-standalone)

> ⚠️ KapitelShelf relies on a PostgreSQL database, but the docker images **do not** include a database server - you must configure your own PostgreSQL connection via environment variables.

> ℹ️ If you'd prefer not to install and manage PostgreSQL yourself, either the below helm chart or the docker-compose file will automatically set up and configure a PostgreSQL instance for you.

## Helm Chart

Deploy KapitelShelf to your Kubernetes cluster using a Helm chart. This option installs all core components (frontend, API, migrator) and can optionally provision a bundled PostgreSQL database.

For a detailed installation and configuration options see the Helm chart's [README](../helm/kapitelshelf/README.md).

## docker-compose

You can find an example `docker-compose.yaml` file in [examples/docker-compose](../examples/docker-compose/docker-compose.yaml).

## Docker (Standalone)

If you prefer to manage each container yourself.

### Registry

The docker images are published on:

|          | Image                                  | Registries                                                                                                                                                                  |
| -------- | -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Frontend | `thomasmiller01/kapitelshelf-frontend` | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-frontend) • [ghcr.io](https://github.com/thomasmiller01/KapitelShelf/pkgs/container/kapitelshelf-frontend) |
| API      | `thomasmiller01/kapitelshelf-api`      | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-api) • [ghcr.io](https://github.com/thomasmiller01/KapitelShelf/pkgs/container/kapitelshelf-api)           |

### Frontend

```bash
docker run -d \
    --name=kapitelshelf-frontend \
    -p 5173:5173 \
    -e VITE_KAPITELSHELF_API=http://localhost:5261 \
    --restart unless-stopped \
    thomasmiller01/kapitelshelf-frontend
```

#### Environment Variables

| Environment Variable    | Default                 |
| ----------------------- | ----------------------- |
| `VITE_KAPITELSHELF_API` | `http://localhost:5261` |

### API

```bash
docker run -d \
    --name=kapitelshelf-api \
    -p 5261:5261 \
    -e KapitelShelf__DataDir=./data \
    -e KapitelShelf__Database__Host=host.docker.internal:5432 \
    -e KapitelShelf__Database__Username=kapitelshelf \
    -e KapitelShelf__Database__Password=kapitelshelf \
    -e KapitelShelf__Domain=https://localhost:5261 \
    -v ./data:/var/lib/kapitelshelf/data
    --restart unless-stopped \
    thomasmiller01/kapitelshelf-api
```

#### Environment Variables

| Environment Variable               | Default                      | Settings Path (appsettings.json) |
| ---------------------------------- | ---------------------------- | -------------------------------- |
| `KapitelShelf__DataDir`            | `/var/lib/kapitelshelf/data` | `KapitelShelf.DataDir`           |
| `KapitelShelf__Database__Host`     | `host.docker.internal:5432`  | `KapitelShelf.Database.Host`     |
| `KapitelShelf__Database__Username` | `kapitelshelf`               | `KapitelShelf.Database.Username` |
| `KapitelShelf__Database__Password` | `kapitelshelf`               | `KapitelShelf.Database.Password` |
| `KapitelShelf__Domain`             | `https://localhost:5261`     | `KapitelShelf.Domain`            |
