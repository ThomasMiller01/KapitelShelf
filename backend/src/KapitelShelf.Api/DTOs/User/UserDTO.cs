// <copyright file="UserDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The user dto.
/// </summary>
public class UserDTO
{
    /// <summary>
    /// Gets or sets the locaiton id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string Username { get; set; } = null!;
}
