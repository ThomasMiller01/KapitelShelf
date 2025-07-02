// <copyright file="UsersController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.User;
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
    public async Task<ActionResult<IList<UserDTO>>> GetUsers()
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
    public async Task<ActionResult<UserDTO>> GetUserById(Guid userId)
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
    /// <param name="createUserDto">The create user dto.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<ActionResult<UserDTO>> CreateUser(CreateUserDTO createUserDto)
    {
        try
        {
            var user = await this.logic.CreateUserAsync(createUserDto);

            return CreatedAtAction(nameof(CreateUser), user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating user with username: {Username}", createUserDto?.Username);
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
    /// <param name="user">The updated user.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateBook(Guid userId, UserDTO user)
    {
        try
        {
            var updatedUser = await this.logic.UpdateUserAsync(userId, user);
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

    /// <summary>
    /// Get the last visited books of a user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{userId}/lastvisitedbooks")]
    public async Task<ActionResult<PagedResult<BookDTO>>> GetlastVisitedBooksByUserId(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        try
        {
            var books = await this.logic.GetLastVisitedBooks(userId, page, pageSize);
            if (books is null)
            {
                return NotFound();
            }

            return Ok(books);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching last visited books");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
