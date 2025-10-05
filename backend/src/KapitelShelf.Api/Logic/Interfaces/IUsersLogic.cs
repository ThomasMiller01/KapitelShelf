// <copyright file="IUsersLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.User;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The users logic interface.
/// </summary>
public interface IUsersLogic
{
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<IList<UserDTO>> GetUsersAsync();

    /// <summary>
    /// Retrieves a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<UserDTO?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createUserDto">The dto containing information for the new user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<UserDTO?> CreateUserAsync(CreateUserDTO createUserDto);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">The id of the user to update.</param>
    /// <param name="userDto">The dto containing updated user information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<UserDTO?> UpdateUserAsync(Guid userId, UserDTO userDto);

    /// <summary>
    /// Deletes a user by its id.
    /// </summary>
    /// <param name="userId">The id of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<UserDTO?> DeleteUserAsync(Guid userId);

    /// <summary>
    /// Get the last visited books of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="page">The page to get.</param>
    /// <param name="pageSize">The size of the pages.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<PagedResult<BookDTO>> GetLastVisitedBooksAsync(Guid userId, int page, int pageSize);

    /// <summary>
    /// Get the settings of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<List<UserSettingDTO>> GetSettingsByUserIdAsync(Guid userId);

    /// <summary>
    /// Update the settings of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="settingDto">The setting.</param>
    /// <returns>A task.</returns>
    Task UpdateSettingAsync(Guid userId, UserSettingDTO settingDto);
}
