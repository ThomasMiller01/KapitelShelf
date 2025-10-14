// <copyright file="NotificationType.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models.Notifications;

/// <summary>
/// The notification type.
/// </summary>
public enum NotificationType
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
