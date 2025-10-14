// <copyright file="NotificationsController.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KapitelShelf.Api.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="NotificationsController"/> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="logic">The series logic.</param>
[ApiController]
[Route("notifications")]
public class NotificationsController(ILogger<NotificationsController> logger, INotificationsLogic logic) : ControllerBase
{
    private readonly ILogger<NotificationsController> logger = logger;

    private readonly INotificationsLogic logic = logic;

    /// <summary>
    /// Get all notifications for the current user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A list of notifications.</returns>
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetAllAsync(Guid userId)
    {
        try
        {
            var notifications = await this.logic.GetByUserIdAsync(userId);
            return this.Ok(notifications);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching notifications");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>No content.</returns>
    [HttpPost("readall")]
    public async Task<IActionResult> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var success = await this.logic.MarkAllAsReadAsync(userId);
            if (!success)
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Get a specific notification by id.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>The notification.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDto>> GetByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var notification = await this.logic.GetByIdAsync(id, userId);
            if (notification is null)
            {
                return this.NotFound();
            }

            return this.Ok(notification);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error fetching the notification with id '{NotificationId}'", id);
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>No content.</returns>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, Guid userId)
    {
        try
        {
            var success = await this.logic.MarkAsReadAsync(id, userId);
            if (!success)
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error marking a notification as read");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
