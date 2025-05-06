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

```yaml
services:
  frontend:
    container_name: kapitelshelf-frontend
    image: thomasmiller01/kapitelshelf-frontend:latest
    restart: unless-stopped
    depends_on:
      migrator:
        condition: service_completed_successfully
    environment:
      VITE_KAPITELSHELF_API: http://localhost:5261
    ports:
      - "5173:5173"

  api:
    container_name: kapitelshelf-api
    image: thomasmiller01/kapitelshelf-api:latest
    restart: unless-stopped
    depends_on:
      migrator:
        condition: service_completed_successfully
    environment:
      KapitelShelf__Database__Host: host.docker.internal:5432
      KapitelShelf__Database__Username: kapitelshelf
      KapitelShelf__Database__Password: kapitelshelf
    ports:
      - "5261:5261"
    volumes:
      - kapitelshelf_data:/var/lib/kapitelshelf/data

  migrator:
    container_name: kapitelshelf-migrator
    image: thomasmiller01/kapitelshelf-migrator:latest
    restart: no
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      KapitelShelf__Database__Host: host.docker.internal:5432
      KapitelShelf__Database__Username: kapitelshelf
      KapitelShelf__Database__Password: kapitelshelf

  # ----- database -----
  postgres:
    image: postgres:16.8
    container_name: postgres
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U $${POSTGRES_USER} -d $${POSTGRES_DB}"]
      interval: 10s
      retries: 5
      start_period: 30s
      timeout: 10s
    environment:
      POSTGRES_USER: kapitelshelf
      POSTGRES_PASSWORD: kapitelshelf
      POSTGRES_DB: kapitelshelf
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  kapitelshelf_data:
  postgres_data:
```

## Docker (Standalone)

If you prefer to manage each container yourself.

### Registry

The docker images are published on:

|          | Image                                  | Registries                                                                                                                                                                  |
| -------- | -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Frontend | `thomasmiller01/kapitelshelf-frontend` | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-frontend) • [ghcr.io](https://github.com/thomasmiller01/KapitelShelf/pkgs/container/kapitelshelf-frontend) |
| API      | `thomasmiller01/kapitelshelf-api`      | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-api) • [ghcr.io](https://github.com/thomasmiller01/KapitelShelf/pkgs/container/kapitelshelf-api)           |
| Migrator | `thomasmiller01/kapitelshelf-migrator` | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-migrator) • [ghcr.io](https://github.com/thomasmiller01/KapitelShelf/pkgs/container/kapitelshelf-migrator) |

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
    -e KapitelShelf__Database__Host=host.docker.internal:5432 \
    -e KapitelShelf__Database__Username=kapitelshelf \
    -e KapitelShelf__Database__Password=kapitelshelf \
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

### Migrator

```bash
docker run -d \
    --name=kapitelshelf-migrator \
    -e KapitelShelf__Database__Host=host.docker.internal:5432 \
    -e KapitelShelf__Database__Username=kapitelshelf \
    -e KapitelShelf__Database__Password=kapitelshelf \
    --restart no \
    thomasmiller01/kapitelshelf-migrator
```

#### Environment Variables

| Environment Variable               | Default                     | Settings Path (appsettings.json) |
| ---------------------------------- | --------------------------- | -------------------------------- |
| `KapitelShelf__Database__Host`     | `host.docker.internal:5432` | `KapitelShelf.Database.Host`     |
| `KapitelShelf__Database__Username` | `kapitelshelf`              | `KapitelShelf.Database.Username` |
| `KapitelShelf__Database__Password` | `kapitelshelf`              | `KapitelShelf.Database.Password` |
