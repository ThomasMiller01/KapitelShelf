// <copyright file="IBooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Interface for the books logic.
/// </summary>
public interface IBooksLogic
{
    /// <summary>
    /// Retrieves all books.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="BookDTO"/>.</returns>
    Task<IList<BookDTO>> GetBooksAsync();

    /// <summary>
    /// Retrieves a book by its id.
    /// </summary>
    /// <param name="bookId">The id of the book.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="BookDTO"/> if found; otherwise, <c>null</c>.</returns>
    Task<BookDTO?> GetBookByIdAsync(Guid bookId);

    /// <summary>
    /// Creates a new book.
    /// </summary>
    /// <param name="createBookDTO">The dto containing information for the new book.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="BookDTO"/>; otherwise, <c>null</c> if creation failed.</returns>
    Task<BookDTO?> CreateBookAsync(CreateBookDTO createBookDTO);

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
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ImportResultDTO"/>.</returns>
    Task<ImportResultDTO> ImportBookAsync(IFormFile file);

    /// <summary>
    /// Deletes all files associated with a book.
    /// </summary>
    /// <param name="bookId">The id of the book whose files should be deleted.</param>
    void DeleteFiles(Guid bookId);

    /// <summary>
    /// Cleans up the database by removing orphaned entities such as authors, series, categories, tags, locations, and files.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CleanupDatabase();
}
