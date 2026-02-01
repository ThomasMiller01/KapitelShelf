# Global Settings

Global settings affect the entire KapitelShelf instance rather than a single user.

## Cloud Storage

| Setting                | Default | Description                                                                 | Note                                                                              |
| ---------------------- | ------- | --------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| Enable `rclone bisync` | `false` | Allows two-way synchronization between KapitelShelf and the cloud provider. | _Experimental_, when disabled, `rclone sync` for one-way synchronisation is used. |

## AI

| Setting     | Default               | Description                                                      | Note |
| ----------- | --------------------- | ---------------------------------------------------------------- | ---- |
| AI Provider | `Disable AI features` | Select which AI provider should be used for AI-powered features. |      |

### AI Features

| Feature                    | Description                                                                             |
| -------------------------- | --------------------------------------------------------------------------------------- |
| Import Metadata Generation | Automatically generate metadata using AI (e.g. tags & categories) when importing books. |

### Ollama Provider

| Setting           | Default                             | Description                                               | Note                                                        |
| ----------------- | ----------------------------------- | --------------------------------------------------------- | ----------------------------------------------------------- |
| Ollama Server URL | `http://host.docker.internal:11434` | Specify the URL of the Ollama server.                     | The server must be reachable from the KapitelShelf backend. |
| Model             | `gemma2:9b`                         | The AI model used in Ollama for KapitelShelf AI features. |                                                             |
