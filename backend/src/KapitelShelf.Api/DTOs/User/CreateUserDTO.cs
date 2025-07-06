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

    /// <summary>
    /// Gets or sets the profile image.
    /// </summary>
    public ProfileImageTypeDTO Image { get; set; }

    /// <summary>
    /// Gets or sets the profile color.
    /// </summary>
    public string Color { get; set; } = null!;
}
