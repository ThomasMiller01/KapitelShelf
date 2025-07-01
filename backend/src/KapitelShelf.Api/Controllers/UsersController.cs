// <copyright file="UsersController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.Logic;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="UsersController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The books logic.</param>
[ApiController]
[Route("users")]
public class UsersController(ILogger<BooksController> logger, UsersLogic logic) : ControllerBase
{
    private readonly ILogger<BooksController> logger = logger;

    private readonly UsersLogic logic = logic;

    /// <summary>
    /// Fetch all users.
    /// </summary>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<ActionResult<IList<BookDTO>>> GetUsers()
    {
        try
        {
            var users = await this.logic.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching users");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get user by the id.
    /// </summary>
    /// <param name="userId">The id of the user to get.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<BookDTO>> GetUserById(Guid userId)
    {
        try
        {
            var user = await this.logic.GetUserByIdAsync(userId);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching user");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="createBookDto">The create user dto.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<ActionResult<BookDTO>> CreateUser(CreateBookDTO createBookDto)
    {
        try
        {
            var user = await this.logic.CreateUserAsync(createBookDto);

            return CreatedAtAction(nameof(CreateUser), user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating user with username: {Username}", createBookDto?.Title);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="userId">The id of the user to delete.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var user = await this.logic.DeleteUserAsync(userId);
            if (user is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting user with Id: {UserId}", userId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="userId">The id of the user to update.</param>
    /// <param name="book">The updated book.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateBook(Guid userId, BookDTO book)
    {
        try
        {
            var updatedUser = await this.logic.UpdateUserAsync(userId, book);
            if (updatedUser is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating user with Id: {UserId}", userId);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
