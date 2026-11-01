# Migrating KapitelShelf

KapitelShelf releases can require configuration or data changes. Starting with `0.4.0`, the frontend, API, Android app, and Helm chart share one version. The API image applies pending EF Core migrations before starting. Each release that needs migration steps has its own section below.

Before upgrading any installation, back up the database and API data directory. Update the frontend and API from the same release; no manual schema migration is normally needed. This guide records the current Helm database change, which does not change the database used by Docker Compose or standalone installations.

## Migrating to v0.4.0

### Helm database default changes

In v0.4.0, the chart changes the bundled database from the deprecated Bitnami PostgreSQL subchart (PostgreSQL 17) to the official PostgreSQL image (PostgreSQL 18), enabled by default. The new database uses a different persistent volume; its volume cannot be used directly by the old server.

This change affects Helm installations as described below; Docker Compose and standalone installations do not change their PostgreSQL server.

#### External PostgreSQL

No data dump is needed. Keep the database name and credentials, set `global.deployDatabase` and `global.deployPostgresql` to `false`, and move the connection settings:

| Previous setting                  | New setting              |
| --------------------------------- | ------------------------ |
| `postgresql.auth.username`        | `database.auth.username` |
| `postgresql.auth.password`        | `database.auth.password` |
| `postgresql.primary.service.host` | `database.service.host`  |
| `postgresql.primary.service.port` | `database.service.port`  |

KapitelShelf connects to a database named `kapitelshelf`. Keep `database.name` set to that value.

```yaml
global:
  deployDatabase: false
  deployPostgresql: false

database:
  name: kapitelshelf
  auth:
    username: <CURRENT_POSTGRESQL_USERNAME>
    password: <CURRENT_POSTGRESQL_PASSWORD>
  service:
    host: <CURRENT_POSTGRESQL_HOST>
    port: 5432
```

#### Bitnami PostgreSQL

The bundled server uses PostgreSQL 18 and a new PVC. Migrate with a logical dump and restore; never attach the Bitnami PVC. Schedule downtime, block user traffic, and back up both the database and API data PVC (by default, `kapitelshelf-data`). Keep the Bitnami PVC and dump until the new database is verified.

The commands use the chart defaults: workload namespace `kapitelshelf`, database user `kapitelshelf`, and new pod `kapitelshelf-database-0`. Run `helm list -A` to find your release name and Helm namespace; use them for `<release>` and `<release-namespace>`. The Bitnami subchart runs in the Helm namespace; the API and new bundled database run in `global.namespace`. The old Bitnami pod depends on the release, so the first command lists it. If you changed `global.namespace`, `nameOverride`, or the database username, use those configured values. Keep the existing database password.

1. **Export the database.** Run in Bash and replace the placeholders:

```bash
kubectl get pods --namespace '<release-namespace>' \
  --selector "app.kubernetes.io/instance=<release>,app.kubernetes.io/name=postgresql,app.kubernetes.io/component=primary"
OLD_DATABASE_POD='<pod name from the output above>'
kubectl exec --stdin --tty --namespace '<release-namespace>' "$OLD_DATABASE_POD" -- \
  pg_dump --username kapitelshelf --dbname kapitelshelf --format custom --file /tmp/kapitelshelf.dump
kubectl cp "<release-namespace>/$OLD_DATABASE_POD:/tmp/kapitelshelf.dump" ./kapitelshelf.dump
pg_restore --list ./kapitelshelf.dump > /dev/null
```

2. **Switch to the bundled database.** Remove the old `postgresql:` subchart values, add this block to your existing values file with the same credentials, then upgrade to chart v0.4.0:

```yaml
global:
  deployDatabase: true
  deployPostgresql: false

database:
  name: kapitelshelf
  auth:
    username: kapitelshelf
    password: <CURRENT_POSTGRESQL_PASSWORD>
```

```bash
helm upgrade '<release>' KapitelShelf/KapitelShelf \
  --namespace '<release-namespace>' --version 0.4.0 --values values.yaml
```

Keep traffic blocked. After Helm returns, stop the API and wait for the new database:

```bash
kubectl scale deployment/kapitelshelf-api --replicas=0 --namespace kapitelshelf
kubectl rollout status deployment/kapitelshelf-api --namespace kapitelshelf --timeout=5m
kubectl rollout status statefulset/kapitelshelf-database --namespace kapitelshelf --timeout=10m
```

```bash
kubectl cp ./kapitelshelf.dump kapitelshelf/kapitelshelf-database-0:/tmp/kapitelshelf.dump
kubectl exec --namespace kapitelshelf kapitelshelf-database-0 -- \
  pg_restore --username kapitelshelf --dbname kapitelshelf \
  --clean --if-exists --no-owner --no-acl --exit-on-error --single-transaction \
  /tmp/kapitelshelf.dump

kubectl scale deployment/kapitelshelf-api --replicas=1 --namespace kapitelshelf
kubectl rollout status deployment/kapitelshelf-api --namespace kapitelshelf --timeout=5m
```

The API applies pending schema migrations when it starts. Verify API health and library contents before allowing traffic. If restore fails, keep the API stopped. Before new writes reach PostgreSQL 18, you can switch back to Bitnami; afterward, restore the backup or use a separately tested reverse migration. Never use the PostgreSQL 18 data directory with PostgreSQL 17.
