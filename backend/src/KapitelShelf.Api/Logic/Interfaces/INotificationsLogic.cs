// <copyright file="INotificationsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The notifications logic interface.
/// </summary>
public interface INotificationsLogic
{
    /// <summary>
    /// Add a notification from a localization key.
    /// </summary>
    /// <param name="localizationKey">The localization key.</param>
    /// <param name="titleArgs">The title arguments.</param>
    /// <param name="messageArgs">The message arguments.</param>
    /// <param name="type">The type.</param>
    /// <param name="severity">The severity.</param>
    /// <param name="expires">The expiry date.</param>
    /// <param name="source">The source.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="parentId">The parent id.</param>
    /// <returns>A task.</returns>
    /// <remarks>If userId is null, the notification will be added to all users.</remarks>
    Task AddNotification(
        string localizationKey,
        object[]? titleArgs = null,
        object[]? messageArgs = null,
        NotificationTypeDto type = NotificationTypeDto.Info,
        NotificationSeverityDto severity = NotificationSeverityDto.Low,
        DateTime? expires = null,
        string source = "",
        Guid? userId = null,
        Guid? parentId = null);

    /// <summary>
    /// Add a notification.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="type">The type.</param>
    /// <param name="severity">The severity.</param>
    /// <param name="expires">The expiry date.</param>
    /// <param name="source">The source.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="parentId">The parent id.</param>
    /// <returns>A task.</returns>
    /// <remarks>If userId is null, the notification will be added to all users.</remarks>
    Task AddNotification(
        string title,
        string message,
        NotificationTypeDto type = NotificationTypeDto.Info,
        NotificationSeverityDto severity = NotificationSeverityDto.Low,
        DateTime? expires = null,
        string source = "",
        Guid? userId = null,
        Guid? parentId = null);

    /// <summary>
    /// Get all notifications for the current user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A list of notifications.</returns>
    Task<List<NotificationDto>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A bool indicating whether the operation was successful.</returns>
    Task<bool> MarkAllAsReadAsync(Guid userId);

    /// <summary>
    /// Get a specific notification by id.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>The notification.</returns>
    Task<NotificationDto?> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="userId">The user id.</param>
    /// <returns>A bool indicating whether the operation was successful.</returns>
    Task<bool> MarkAsReadAsync(Guid id, Guid userId);

    /// <summary>
    /// Delete all expired notifications.
    /// </summary>
    /// <returns>A task.</returns>
    Task DeleteExpiredNotificationsAsync();

    /// <summary>
    /// Gets the current notification stats.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The notification stats.</returns>
    Task<NotificationStatsDto> GetStatsAsync(Guid userId);
}
