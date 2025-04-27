# kapitelshelf

KapitelShelf helm chart for deploying the KapitelShelf application stack, including optional PostgreSQL database support. Provides a flexible setup for both self-contained and external database configurations.

![Version: 0.0.0](https://img.shields.io/badge/Version-0.0.0-informational?style=flat-square)

**Homepage:** <https://github.com/ThomasMiller01/KapitelShelf>

## Additional Information

This Helm chart is primarily intended to deploy **KapitelShelf**, including all necessary components like the frontend, API, and migrator components.

Additionally, the chart can deploy a **PostgreSQL** database alongside KapitelShelf by enabling the bundled [Bitnami PostgreSQL](https://artifacthub.io/packages/helm/bitnami/postgresql) subchart. This provides a fully self-contained deployment without requiring any external database setup.

By default, PostgreSQL is **enabled**. 
However, if you already have your own PostgreSQL instance available, you can configure KapitelShelf to connect to your external database instead.

To use your own database:
- Set `global.deployPostgresql=false`
- Provide the external database connection information to KapitelShelf via Helm values.

## Installing the Chart

To install the chart with the release name `my-release`:

```console
$ helm repo add TODO http://charts.TODO.com
$ helm install my-release TODO/kapitelshelf
```

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 16.6.6 |

## Values

### Api Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| api.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| api.image.repository | string | `"thomasmiller01/kapitelshelf-api"` | Docker image repository |
| api.image.tag | string | `"0.1.0"` | Docker image tag |
| api.resources.limits | object | `{"cpu":"500m","memory":"512Mi"}` | Sets the api container resources limits |
| api.resources.requests | object | `{"cpu":"250m","memory":"256Mi"}` | Sets the api container resources requests |
| api.service.port | int | `5261` | Api port |

### Frontend Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| frontend.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| frontend.image.repository | string | `"thomasmiller01/kapitelshelf-frontend"` | Docker image repository |
| frontend.image.tag | string | `"0.1.0"` | Docker image tag |
| frontend.resources.limits | object | `{"cpu":"500m","memory":"512Mi"}` | Sets the frontend container resources limits   |
| frontend.resources.requests | object | `{"cpu":"250m","memory":"256Mi"}` | Sets the frontend container resources requests  |
| frontend.service.port | int | `5173` | Frontend port |

### Migrator Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| migrator.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| migrator.image.repository | string | `"thomasmiller01/kapitelshelf-migrator"` | Docker image repository |
| migrator.image.tag | string | `"0.1.0"` | Docker image tag |
| migrator.resources.limits | object | `{"cpu":"500m","memory":"512Mi"}` | Sets the migrator container resources limits |
| migrator.resources.requests | object | `{"cpu":"250m","memory":"256Mi"}` | Sets the migrator container resources limits |

### PostgreSQL Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| postgresql.primary.resources.limits | object | `{"memory":"2Gi"}` | Sets the postgres container resources limits |
| postgresql.primary.service.host | string | `nil` | Sets the postgres host, if you're using an external postgres. Will be ignored, if `global.deployPostgres=true`. |

### Other Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| global.deployPostgres | bool | `true` |  |
| ingress.annotations."kubernetes.io/ingress.class" | string | `"nginx"` |  |
| ingress.enabled | bool | `false` |  |
| ingress.hosts[0].host | string | `"kapitelshelf.example.com"` |  |
| ingress.hosts[0].paths[0].path | string | `"/api"` |  |
| ingress.hosts[0].paths[0].port | int | `5261` |  |
| ingress.hosts[0].paths[0].service | string | `"api"` |  |
| ingress.hosts[0].paths[1].path | string | `"/"` |  |
| ingress.hosts[0].paths[1].port | int | `5173` |  |
| ingress.hosts[0].paths[1].service | string | `"frontend"` |  |
| ingress.tls | list | `[]` |  |
| postgresql.auth.database | string | `"kapitelshelf"` |  |
| postgresql.auth.password | string | `"kapitelshelf"` |  |
| postgresql.auth.username | string | `"kapitelshelf"` |  |
| postgresql.primary.persistence.size | string | `"50Gi"` |  |
| postgresql.primary.service.port | int | `5432` |  |

## Source Code

* <https://github.com/ThomasMiller01/KapitelShelf/tree/main/helm/kapitelshelf>

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)