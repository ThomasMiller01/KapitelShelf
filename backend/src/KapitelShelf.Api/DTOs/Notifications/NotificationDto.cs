// <copyright file="NotificationDto.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.Notifications;

/// <summary>
/// The category dto.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public NotificationTypeDto Type { get; set; }

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public NotificationSeverityDto Severity { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the expires date.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification was read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets the source of the notificaiton.
    /// </summary>
    public string Source { get; set; } = null!;

    /// <summary>
    /// Gets or sets the children.
    /// </summary>
    public List<NotificationDto> Children { get; set; } = [];
}
