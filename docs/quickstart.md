# Quickstart Guide

**Welcome to KapitelShelf!**

This quickstart guide shows you how to begin using the app and manage your book collection.

## 1. Access the KapitelShelf frontend

Open your browser and navigate to:

```
http://localhost:5173
```

> ℹ️ The url can **vary** based on how you installed it. <br /> **docker-compose** & **Docker** => use the url provided above <br /> **Helm** => navigate to the url configured via the `frontend.ingress` values

## 2. Add Your First Book

1. Click the `+` button on the top right.

![Create Dialog](./.attachments/references/add_book/manual/create_dialog.png)

2. Click the `Create Book` button _(directly below)_.

![Create Book Button](./.attachments/references/add_book/manual/create_book.png)

3. Fill out the book details: _title_, _description_, ...
4. _**[Optional]**_ Import metadata as described in [Import Metadata for a Book](./references.md#import-metadata-for-a-book).
5. Click the `Create Book` button on the bottom right.

Your book now appears in your collection.

## 3. Browse Your Collection

After adding books, you can visit your personal library.

1. Click on `Library` to see your book collection

![Visit Library](./.attachments/references/library/visit_library.png)

> ℹ️ On the library page, you’ll see all your series listed. <br /> To view the books in a specific series, see [4. View Series Details](#4-view-series-details).

## 4. View Series Details

On the library page, click on any series from the list to open its details page.

You'll now see all books that belong to this series, displayed in order.

## See Book Details

On the series details page, click on any book from the list to open its details page.

Here you can see all information about the book, including title, description, cover image and more.

## 5. Search Books

Use the **search bar** at the top of the page to find books in your collection by title, author or keyword.

![Search Bar](./.attachments/references/search/search_bar.png)

## 6. Edit a Book (or Series)

Visit the book (or series) details page and click the `Edit` _(pencil icon)_ button.

![Edit Button](./.attachments/references/edit_book/edit_button.png)

Now edit the book details: _title_, _description_, ...

Click the `Edit Book` button on the bottom right to save your changes.

## 7. Delete a Book (or Series)

Visit the book (or series) details page and click the `Delete` _(trash icon)_ button.

![Delete Button](./.attachments/references/delete_book/delete_button.png)

Now **confirm** the deletion in the dialog via the red `Delete` button.

![Delete Dialog](./.attachments/references/delete_book/delete_dialog.png)

## Tips

- **Responsive design:** KapitelShelf works on desktop and mobile.
- **Book files:** Supported formats include EPUB, FB2, PDF and more.

## Help and Support

- For questions, visit the [General](https://github.com/ThomasMiller01/KapitelShelf/discussions/categories/general) section of the discussions on GitHub.
- For bugs, [open an issue](https://github.com/ThomasMiller01/KapitelShelf/issues) on GitHub.
- Check the [References](#) for advanced usage and more details. _(planned)_

---

**🎉 Enjoy organizing your library with KapitelShelf!**
