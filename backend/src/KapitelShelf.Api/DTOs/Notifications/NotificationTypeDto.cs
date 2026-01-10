// <copyright file="NotificationTypeDto.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Notifications;

/// <summary>
/// The notification type dto.
/// </summary>
public enum NotificationTypeDto
{
    /// <summary>
    /// Information for the user.
    /// </summary>
    Info,

    /// <summary>
    /// Successful operation.
    /// </summary>
    Success,

    /// <summary>
    /// Warn the user.
    /// </summary>
    Warning,

    /// <summary>
    /// Notify about an error.
    /// </summary>
    Error,

    /// <summary>
    /// System notification, maintenance.
    /// </summary>
    System,
}
