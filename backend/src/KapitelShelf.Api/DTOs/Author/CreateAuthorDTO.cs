// <copyright file="CreateAuthorDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Author;

/// <summary>
/// The create dto for a author.
/// </summary>
public class CreateAuthorDTO
{
    /// <summary>
    /// Gets or sets or set the first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = null!;
}
