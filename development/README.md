# Setup Development Environment

VSCode Tasks:

- **Dev: Run All** -> start development environment locally

## Requirements

- docker

Strongly suggest:

- use VSCode for frontend / building
- use VS for backend

- helm
- [helm-docs](https://github.com/norwoodj/helm-docs)

### Mobile development requirements

If you want to build and run the mobile app locally, you also need:

- Java JDK 17 (required for Android/Gradle)
- Android Studio (includes SDK, emulator, platform tools)
- CocoaPods (for iOS, only on macOS)

➡️ See [SetupForMobile.md](./SetupForMobile.md) for full setup and build instructions.

## Swagger

Generate swagger json

```bash
dotnet tool install --global Swashbuckle.AspNetCore.Cli --version 8.1.2
```
