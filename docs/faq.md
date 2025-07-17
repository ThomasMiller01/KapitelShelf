# FAQ

## Why do I need to create my own Microsoft Azure Application when using OneDrive?

The authentication for OneDrive uses the Microsoft OAuth Authentication Code Flow. The redirection endpoint which has to be configured for this flow to work has to be an **absolute** URI pointing to the KapitelShelf API.

This URI is not allowed to be any kind of wildcard, see specification [RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2).

Because KapitelShelf allows its API to be hosted behind any domain, it is not possible to use a single Azure Application for the entire KapitelShelf community.

> Tools like [rclone](https://rclone.org/) ca use a "default" Azure Application, because their server always runs locally under a specific url (like `http://127.0.0.1:53682`)
