# FAQ

## Why is KapitelShelf only available on Android and not on iOS?

Creating an Android app that can be installed on real devices is **completely free**. However, creating an iOS app that can be installed on real devices requires enrollment in the [Apple Developer Program](https://developer.apple.com/programs/), **which costs $99 per year**.

Since KapitelShelf is an open-source project that I maintain in my free time, I cannot cover this rocurring cost. For that reason, only an Android app is currently provided (in addition to the web frontend).

> iOS users can still access the fully functional web frontend of KapitelShelf, which works on all modern browsers.

## Why do I need to create my own Microsoft Azure Application when using OneDrive?

The authentication for OneDrive uses the Microsoft OAuth Authentication Code Flow. The redirection endpoint which has to be configured for this flow to work has to be an **absolute** URI pointing to the KapitelShelf API.

This URI is not allowed to be any kind of wildcard, see specification [RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2).

Because KapitelShelf allows its API to be hosted behind any domain, it is not possible to use a single Azure Application for the entire KapitelShelf community.

> Tools like [rclone](https://rclone.org/) can use a "default" Azure Application, as their server always runs locally under a specific url (like `http://127.0.0.1:53682`), instead of being configurable (like at KapitelShelf).
