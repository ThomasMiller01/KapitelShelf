# KapitelShelf

KapitelShelf helm chart for deploying the KapitelShelf application stack, including optional PostgreSQL database support. Provides a flexible setup for both self-contained and external database configurations.

![Version: 0.3.4](https://img.shields.io/badge/Version-0.3.4-informational?style=flat-square) 

**Homepage:** <https://github.com/ThomasMiller01/KapitelShelf>

## Additional Information

This Helm chart is primarily intended to deploy **KapitelShelf**, including all necessary components like the frontend and API components.

### Database

Additionally, the chart can deploy a **PostgreSQL** database alongside KapitelShelf by enabling the bundled [Bitnami PostgreSQL](https://artifacthub.io/packages/helm/bitnami/postgresql) subchart. This provides a fully self-contained deployment without requiring any external database setup.

By default, PostgreSQL is **enabled**.  
However, if you already have your own PostgreSQL instance available, you can configure KapitelShelf to connect to your external database instead.

To use your own database:

- Set `global.deployPostgresql=false`
- Provide the external database connection information to KapitelShelf via Helm values.

### AI

Optional AI‑powered features can be deployed alongide KapitelShelf by enabling the bundled [Otwld Ollama](https://github.com/otwld/ollama-helm) subchart. These features are disabled by default and depend on an external AI service, as KapitelShelf itself does _not_ host or run any models.

To use the bundled Ollama subchart:

- Set `global.deployOllama=true`

**Additionally**, refer to the [AI quickstart documentation](../../docs/ai.md) for the **remaining setup.**

> The **Ollama Server Url** in this case would be ``.

## Installing the Chart

To install the chart with the release name `my-release`:

```console
$ helm repo add KapitelShelf https://thomasmiller01.github.io/KapitelShelf/helm
$ helm install my-release KapitelShelf/KapitelShelf
```

You can find example `values.yaml` configuration in [KapitelShelf Examples](https://github.com/ThomasMiller01/KapitelShelf/tree/main/examples/helm).

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 16.6.6 |
| https://helm.otwld.com | ollama | 1.48.0 |

## Values

### Api Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| api.image.pullPolicy | string | `"IfNotPresent"` | Docker [imagePullPolicy](https://kubernetes.io/docs/concepts/containers/images/#pre-pulled-images) |
| api.image.repository | string | `"thomasmiller01/kapitelshelf-api"` | Docker image repository |
| api.image.tag | string | `"0.3.5"` | Docker image tag |
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
| frontend.image.tag | string | `"0.3.4"` | Docker image tag |
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
| global.deployOllama | bool | `false` | Whether to deploy a bundled Ollama instance using the Otwld Ollama subchart.<br /> Set to `true` if you want to use AI features with selfhosted Ollama.<br /> See [AI: Ollama Provider](../../docs/ai.md#ollama-provider) for more information. |
| global.deployPostgresql | bool | `true` | Whether to deploy a bundled PostgreSQL instance using the Bitnami PostgreSQL subchart.<br /> Set to `false` if you want KapitelShelf to connect to your own external PostgreSQL database |
| global.namespace | string | `"kapitelshelf"` | The helm chart will be deployed into this namespace |
| global.storage.size | string | `"20Gi"` | Size of the KapitelShelf storage |

### Ollama Values _[SubChart]_

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| ollama.gpu.enabled | bool | `false` | Enable GPU integration, see [Otwld Ollama Helm Values](https://github.com/otwld/ollama-helm?tab=readme-ov-file#helm-values) for more information |
| ollama.persistentVolume.size | string | `"20Gi"` | Size of the Ollama storage |
| ollama.resources.limits | object | `{"memory":"8Gi"}` | Sets the Ollama container resources limits |
| ollama.resources.requests | object | `{"memory":"4Gi"}` | Sets the Ollama container resources requests |

### PostgreSQL Values _[SubChart]_

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| postgresql.auth.password | string | `"kapitelshelf"` | PostgreSQL database password **(Change this for production!)** |
| postgresql.auth.username | string | `"kapitelshelf"` | PostgreSQL database username **(Change this for production!)** |
| postgresql.primary.persistence.size | string | `"20Gi"` | Size of the PostgreSQL storage |
| postgresql.primary.resources.limits | object | `{"memory":"2Gi"}` | Sets the PostgreSQL container resources limits |
| postgresql.primary.service.host | string | `nil` | Sets the PostgreSQL host, if you're using an external PostgreSQL.<br /> Will be ignored, if `global.deployPostgres=true` |
| postgresql.primary.service.port | int | `5432` | TCP port the PostgreSQL service will listen on |

## Source Code

* <https://github.com/ThomasMiller01/KapitelShelf/tree/main/helm/kapitelshelf>


----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
