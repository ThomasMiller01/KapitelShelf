// <copyright file="IBooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// Interface for the books logic.
/// </summary>
public interface IBooksLogic
{
    /// <summary>
    /// Retrieves all books.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="BookDTO"/>.</returns>
    [Obsolete("Use GetBooksAsync() instead", false)]
    Task<IList<BookDTO>> GetBooksAsyncDeprecated();

    /// <summary>
    /// Retrieves all books.
    /// </summary>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <param name="sortBy">Sort the books by this field.</param>
    /// <param name="sortDir">Sort the books in this direction.</param>
    /// <param name="filter">Filter the books.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="BookDTO"/>.</returns>
    Task<PagedResult<BookDTO>> GetBooksAsync(int page, int pageSize, BookSortByDTO sortBy, SortDirectionDTO sortDir, string? filter);

    /// <summary>
    /// Retrieves a book by its id.
    /// </summary>
    /// <param name="bookId">The id of the book.</param>
    /// <param name="userId">The id of the user that is accessing this book.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="BookDTO"/> if found; otherwise, <c>null</c>.</returns>
    Task<BookDTO?> GetBookByIdAsync(Guid bookId, Guid? userId = null);

    /// <summary>
    /// Search all books by search term.
    /// </summary>
    /// <param name="searchterm">The search term.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A <see cref="Task{IList}"/> representing the result of the asynchronous operation.</returns>
    Task<PagedResult<BookDTO>> Search(string searchterm, int page, int pageSize);

    /// <summary>
    /// Creates a new book.
    /// </summary>
    /// <param name="createBookDTO">The dto containing information for the new book.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="BookDTO"/>; otherwise, <c>null</c> if creation failed.</returns>
    Task<BookDTO?> CreateBookAsync(CreateBookDTO createBookDTO);

    /// <summary>
    /// Delete books in bulk.
    /// </summary>
    /// <param name="bookIdsToDelete">The book ids to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteBooksAsync(List<Guid> bookIdsToDelete);

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    /// <param name="bookId">The id of the book to update.</param>
    /// <param name="bookDto">The dto containing updated book information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="BookDTO"/>; otherwise, <c>null</c> if the book was not found.</returns>
    Task<BookDTO?> UpdateBookAsync(Guid bookId, BookDTO bookDto);

    /// <summary>
    /// Deletes a book by its id.
    /// </summary>
    /// <param name="bookId">The id of the book to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deleted <see cref="BookDTO"/>; otherwise, <c>null</c> if the book was not found.</returns>
    Task<BookDTO?> DeleteBookAsync(Guid bookId);

    /// <summary>
    /// Imports a book from a file.
    /// </summary>
    /// <param name="file">The file containing the book to import.</param>
    /// <param name="userId">The id of the user triggering this action.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ImportResultDTO"/>.</returns>
    Task<ImportResultDTO> ImportBookAsync(IFormFile file, Guid? userId = null);

    /// <summary>
    /// Imports a book from an asin.
    /// </summary>
    /// <param name="asin">The asin of the book to import.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ImportResultDTO"/>.</returns>
    Task<ImportResultDTO?> ImportBookFromAsinAsync(string asin);

    /// <summary>
    /// Deletes all files associated with a book.
    /// </summary>
    /// <param name="bookId">The id of the book whose files should be deleted.</param>
    void DeleteFiles(Guid bookId);

    /// <summary>
    /// Check if the passed file is already imported as a book.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file is already imported, otherwise false.</returns>
    Task<bool> BookFileExists(IFormFile file);

    /// <summary>
    /// Get the autocomplete result for the series.
    /// </summary>
    /// <param name="partialSeriesName">The partial series name.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteSeriesAsync(string? partialSeriesName);

    /// <summary>
    /// Get the autocomplete result for the author.
    /// </summary>
    /// <param name="partialAuthor">The partial author.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteAuthorAsync(string? partialAuthor);

    /// <summary>
    /// Get the autocomplete result for the category.
    /// </summary>
    /// <param name="partialCategoryName">The partial category name.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteCategoryAsync(string? partialCategoryName);

    /// <summary>
    /// Get the autocomplete result for the tag.
    /// </summary>
    /// <param name="partialTagName">The partial tag name.</param>
    /// <returns>The autocomplete result.</returns>
    Task<List<string>> AutocompleteTagAsync(string? partialTagName);

    /// <summary>
    /// Cleans up the database by removing orphaned entities such as authors, series, categories, tags, locations, and files.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CleanupDatabase();
}
