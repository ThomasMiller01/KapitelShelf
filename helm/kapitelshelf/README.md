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
| api.ingress.annotations | object | `{}` | Additional annotations<br /> e.g. `cert-manager.io/cluster-issuer: "letsencrypt-prod"` |
| api.ingress.enabled | bool | `false` | Whether to enable an ingress resource for the api |
| api.ingress.host | string | `nil` | Hostname for the api ingress<br /> e.g. `"api.example.com"` |
| api.ingress.path | string | `"/"` | Path under the host to route to the api service |
| api.ingress.tls | list | `[]` | TLS configuration for the Api ingress<br /> e.g.<br /> tls:<br />   - hosts:<br />       - api.example.com<br /> |
| api.resources.limits | object | `{"cpu":"500m","memory":"1Gi"}` | Sets the api container resources limits |
| api.resources.requests | object | `{"cpu":"250m","memory":"512Mi"}` | Sets the api container resources requests |
| api.service.port | int | `5261` | Api port |

### Frontend Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| frontend.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| frontend.image.repository | string | `"thomasmiller01/kapitelshelf-frontend"` | Docker image repository |
| frontend.image.tag | string | `"0.1.0"` | Docker image tag |
| frontend.ingress.annotations | object | `{}` | Additional annotations<br /> e.g. `cert-manager.io/cluster-issuer: "letsencrypt-prod"` |
| frontend.ingress.enabled | bool | `false` | Whether to enable an ingress resource for the frontend |
| frontend.ingress.host | string | `nil` | Hostname for the frontend ingress<br /> e.g. `"frontend.example.com"` |
| frontend.ingress.path | string | `"/"` | Path under the host to route to the frontend service |
| frontend.ingress.tls | list | `[]` | TLS configuration for the frontend ingress<br /> e.g.<br /> tls:<br />   - hosts:<br />       - frontend.example.com<br /> |
| frontend.resources.limits | object | `{"cpu":"500m","memory":"512Mi"}` | Sets the frontend container resources limits   |
| frontend.resources.requests | object | `{"cpu":"50m","memory":"128Mi"}` | Sets the frontend container resources requests  |
| frontend.service.port | int | `5173` | Frontend port |

### Global Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| global.deployPostgres | bool | `true` | Whether to deploy a bundled PostgreSQL instance using the Bitnami PostgreSQL subchart.<br /> Set to `false` if you want KapitelShelf to connect to your own external PostgreSQL database |

### Migrator Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| migrator.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| migrator.image.repository | string | `"thomasmiller01/kapitelshelf-migrator"` | Docker image repository |
| migrator.image.tag | string | `"0.1.0"` | Docker image tag |
| migrator.resources.limits | object | `{"cpu":"200m","memory":"256Mi"}` | Sets the migrator container resources limits |
| migrator.resources.requests | object | `{"cpu":"100m","memory":"128Mi"}` | Sets the migrator container resources limits |

### PostgreSQL Values _[SubChart]_

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| postgresql.auth.password | string | `"kapitelshelf"` | PostgreSQL database password **(Change this for production!)** |
| postgresql.auth.username | string | `"kapitelshelf"` | PostgreSQL database username **(Change this for production!)** |
| postgresql.primary.persistence.size | string | `"50Gi"` | Size of the PostgreSQL storage |
| postgresql.primary.resources.limits | object | `{"memory":"2Gi"}` | Sets the PostgreSQL container resources limits |
| postgresql.primary.service.host | string | `nil` | Sets the PostgreSQL host, if you're using an external PostgreSQL.<br /> Will be ignored, if `global.deployPostgres=true` |
| postgresql.primary.service.port | int | `5432` | TCP port the PostgreSQL service will listen on |

## Source Code

* <https://github.com/ThomasMiller01/KapitelShelf/tree/main/helm/kapitelshelf>

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)