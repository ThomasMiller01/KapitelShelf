// <copyright file="CreateUserDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The create dto for a user.
/// </summary>
public class CreateUserDTO
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = null!;
}
