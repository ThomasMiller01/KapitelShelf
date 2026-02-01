# AI Quickstart in KapitelShelf

KapitelShelf supports optional AI-powered features such as **semantic search** and **automatic metadata generation**.

> ℹ️ Important
>
> - KapitelShelf **does not run AI models itself**
> - You must provide and run an external AI service

> ⚠️ Current limitation  
> At the moment, **only Ollama is supported**.  
> Support for OpenAI and additional providers will be added in the future.

## What you need

To use AI features in KapitelShelf, you must set up **one supported AI provider** and make it reachable by the KapitelShelf backend.

### Ollama provider

You need a **running Ollama server**, which can be hosted:

- locally on your machine
- on a server in your network
- inside a Docker container

KapitelShelf does not care **where** Ollama runs, it only needs an HTTP endpoint it can access.

For docker-based setups, see the official Ollama documentation: [docs.ollama.com](https://docs.ollama.com/docker).

## Configure AI in KapitelShelf

Once Ollama is running, configure it in KapitelShelf:

In **KapitelShelf -> Settings -> AI**:

1. Enable AI features
2. Select provider: **Ollama**
3. Enter the Ollama server URL, for example: `http://host.docker.internal:11434`
4. KapitelShelf will automatically try the configuration

No API key is required when using Ollama.

## How to verify AI features work

If the configuration is valid, a green **"Configured"** badge will be shown:

![AI Configured](./.attachments/ai/ai_configured.png)

You can now use AI-powered features such as semantic search.

## Troubleshooting

If AI features do not work as expected:

- Ensure the Ollama server is running
- Verify the configured URL is reachable from the KapitelShelf backend
- Check the KapitelShelf backend logs for connection or timeout errors
