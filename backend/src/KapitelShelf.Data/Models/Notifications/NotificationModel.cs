// <copyright file="NotificationModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data.Models.User;

namespace KapitelShelf.Data.Models.Notifications;

/// <summary>
/// The category model.
/// </summary>
public class NotificationModel
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
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Low;

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
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public virtual UserModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the source of the notificaiton.
    /// </summary>
    public string Source { get; set; } = null!;
}
