# Quickstart Guide

Welcome to KapitelShelf! This guide walks you through the first essential actions so you can start organising your library within minutes. If you have not set up the application yet, begin with the [Installation Guide](./installation.md) before continuing.

## 1. Launch the Frontend

Open your browser and navigate to your KapitelShelf instance.

```text
http://localhost:5173
```

> ℹ️ The exact URL depends on how KapitelShelf was installed. <br /> **docker-compose** or **Docker** deployments typically use the address above. <br /> **Helm** deployments should use the URL configured via the `frontend.ingress` value.

## 2. Create Your First User Profile

A user profile stores personalised settings, ratings and reading metadata. Create one to begin using KapitelShelf.

1. Click the **`+` (plus)** button to open the new profile dialog.

   ![Create User Profile Button](./.attachments/references/user_profiles/create_user_profile_button.png)

2. Enter a username and press **Save**.

   ![Create User Profile Dialog](./.attachments/references/user_profiles/create_user_profile_dialog.png)

## 3. Add a Book to Your Library

Start curating your collection by adding a title manually.

1. Click the **`+`** button in the top-right corner.

   ![Create Dialog](./.attachments/references/add_book/manual/create_dialog.png)

2. Choose **Create Book**.

   ![Create Book Button](./.attachments/references/add_book/manual/create_book.png)

3. Fill in the book details such as title and description.
4. _Optional:_ Import richer metadata using the workflow described in [Import Metadata for a Book](./references.md#import-metadata-for-a-book).
5. Click **Create Book** at the bottom-right to save it.

Your new book immediately appears in your personal library.

## 4. Explore Your Library

Browse the collection you have built and dive into each series.

1. Select **Library** from the navigation to see every series you own.

   ![Visit Library](./.attachments/references/library/visit_library.png)

> ℹ️ Series act as containers for books. To focus on a single series, open it from the list and explore the options described in [Library and Search](./references.md#library-and-search).

## Next Steps

- Continue building your collection: import files, upload CSVs or fetch books by ASIN. The [References](./references.md) page covers each method in depth.
- Review the [FAQ](./faq.md) if you have questions about common scenarios.
- Join the community via the [GitHub discussions](https://github.com/ThomasMiller01/KapitelShelf/discussions/categories/general) or report bugs through the [issue tracker](https://github.com/ThomasMiller01/KapitelShelf/issues).

Enjoy organising your reading life with KapitelShelf! 🎉
