// <copyright file="AuthorDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Author;

/// <summary>
/// The Author DTO.
/// </summary>
public class AuthorDTO
{
    /// <summary>
    /// Gets or sets the author id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets or set the first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the total number of books.
    /// </summary>
    public int? TotalBooks { get; set; }
}
