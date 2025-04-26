# Installation Guide

- [docker-compose](#docker-compose)
- [Docker (Standalone)](#docker-standalone)

> ⚠️ KapitelShelf relies on a PostgreSQL database, but the docker images **do not** include a database server - you must configure your own PostgreSQL connection via environment variables.

> ℹ️ If you'd prefer not to install and manage PostgreSQL yourself, the below docker-compose file will automatically set up and configure a PostgreSQL instance for you.

## docker-compose

```yaml
services:
  frontend:
    image: thomasmiller01/kapitelshelf-frontend
    container_name: kapitelshelf-frontend
    restart: unless-stopped
    depends_on:
      migrator:
        condition: service_completed_successfully
    environment:
      KAPITELSHELF_API: host.docker.internal:5261
    ports:
      - "5173:5173"

  api:
    image: thomasmiller01/kapitelshelf-api
    container_name: kapitelshelf-api
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

  migrator:
    image: thomasmiller01/kapitelshelf-migrator
    container_name: kapitelshelf-migrator
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

  pgadmin:
    image: dpage/pgadmin4:9.2.0
    container_name: pgadmin4
    restart: unless-stopped
    environment:
      PGADMIN_DEFAULT_EMAIL: kapitelshelf@example.com
      PGADMIN_DEFAULT_PASSWORD: kapitelshelf
    volumes:
      - ./config/pgadmin4/servers.json:/pgadmin4/servers.json
    ports:
      - "5050:80"
    depends_on:
      - postgres

volumes:
  postgres_data:
```

## Docker (Standalone)

If you prefer to manage each container yourself.

### Registry

The docker images are published on:

|          | Image                                  | Registries                                                                 |
| -------- | -------------------------------------- | -------------------------------------------------------------------------- |
| Frontend | `thomasmiller01/kapitelshelf-frontend` | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-frontend) |
| API      | `thomasmiller01/kapitelshelf-api`      | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-api)      |
| Migrator | `thomasmiller01/kapitelshelf-migrator` | [DockerHub](https://hub.docker.com/r/thomasmiller01/kapitelshelf-migrator) |

### Frontend

```bash
docker run -d \
    --name=kapitelshelf-frontend \
    -p 5173:5173 \
    -e KapitelShelf__Database__Host=localhost:5261 \
    --restart unless-stopped \
    thomasmiller01/kapitelshelf-frontend
```

#### Environment Variables

| Settings Path                | Environment Variable           | Default          |
| ---------------------------- | ------------------------------ | ---------------- |
| `KapitelShelf.Database.Host` | `KapitelShelf__Database__Host` | `localhost:5261` |

### API

```bash
docker run -d \
    --name=kapitelshelf-api \
    -p 5261:5261 \
    -e KapitelShelf__Database__Host=host.docker.internal:5432 \
    -e KapitelShelf__Database__Username=kapitelshelf \
    -e KapitelShelf__Database__Password=kapitelshelf \
    --restart unless-stopped \
    thomasmiller01/kapitelshelf-api
```

#### Environment Variables

| Settings Path                    | Environment Variable               | Default                     |
| -------------------------------- | ---------------------------------- | --------------------------- |
| `KapitelShelf.Database.Host`     | `KapitelShelf__Database__Host`     | `host.docker.internal:5432` |
| `KapitelShelf.Database.Username` | `KapitelShelf__Database__Username` | `kapitelshelf`              |
| `KapitelShelf.Database.Password` | `KapitelShelf__Database__Password` | `kapitelshelf`              |

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

| Settings Path                    | Environment Variable               | Default                     |
| -------------------------------- | ---------------------------------- | --------------------------- |
| `KapitelShelf.Database.Host`     | `KapitelShelf__Database__Host`     | `host.docker.internal:5432` |
| `KapitelShelf.Database.Username` | `KapitelShelf__Database__Username` | `kapitelshelf`              |
| `KapitelShelf.Database.Password` | `KapitelShelf__Database__Password` | `kapitelshelf`              |
