// <copyright file="NotificationStatsDto.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Notifications;

/// <summary>
/// The notification stats dto.
/// </summary>
public class NotificationStatsDto
{
    /// <summary>
    /// Gets or sets the amount of unread messages.
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has unread critical messages.
    /// </summary>
    public bool UnreadHasCritical { get; set; }

    /// <summary>
    /// Gets or sets the total amount of messages.
    /// </summary>
    public int TotalMessages { get; set; }
}
