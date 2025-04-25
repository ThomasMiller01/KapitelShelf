# Create new DB Migration

```pwsh
PS ~\backend\src> dotnet ef migrations add <MigrationName> --project ./KapitelShelf.Data.Migrations --startup-project ./KapitelShelf.Data.Migrations --context KapitelShelfDBContext
```

# Docker

## KapitelShelf.Api

### Environment

ASP .NET Core reads `appsettings.json` first, then any environment variables matching configuration keys (with `:` replaced by `__`) will override those values.

For example, given this in the JSON:

```json
{
  "KapitelShelf": {
    "Database": {
      "Host": "localhost",
      "Username": "kapitelshelf",
      "Password": "kapitelshelf"
    }
  }
}
```

You can override each setting at runtime by setting:

- `KapitelShelf__Database__Host=myotherhost`
- `KapitelShelf__Database__Username=myotherusername`
- `KapitelShelf__Database__Password=myotherpassword`

#### Available Environment Variables:

| JSON Path                        | Environment Variable               | Default Value          |
| -------------------------------- | ---------------------------------- | ---------------------- |
| `KapitelShelf.Database.Host`     | `KapitelShelf__Database__Host`     | `host.docker.internal` |
| `KapitelShelf.Database.Username` | `KapitelShelf__Database__Username` | `kapitelshelf`         |
| `KapitelShelf.Database.Password` | `KapitelShelf__Database__Password` | `kapitelshelf`         |
