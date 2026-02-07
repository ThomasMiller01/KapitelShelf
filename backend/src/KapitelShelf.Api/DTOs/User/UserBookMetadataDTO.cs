// <copyright file="UserBookMetadataDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The book dto.
/// </summary>
public class UserBookMetadataDTO
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    [Range(1, 10)]
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
}
