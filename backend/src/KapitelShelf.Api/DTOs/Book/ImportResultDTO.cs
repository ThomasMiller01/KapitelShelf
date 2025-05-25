// <copyright file="ImportResultDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Book;

/// <summary>
/// Represents the result of an import operation, including details about the success or failure of the process.
/// </summary>
public class ImportResultDTO
{
    /// <summary>
    /// Gets or sets a value indicating whether it was a bulk import.
    /// </summary>
    public bool IsBulkImport { get; set; } = false;

    /// <summary>
    /// Gets or sets any errors encountered during the import process.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets the successfully imported books, containing their IDs and titles.
    /// </summary>
    public List<ImportBookDTO> ImportedBooks { get; set; } = [];
}

/// <summary>
/// The import book dto.
/// </summary>
public class ImportBookDTO
{
    /// <summary>
    /// Gets or sets the book id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
