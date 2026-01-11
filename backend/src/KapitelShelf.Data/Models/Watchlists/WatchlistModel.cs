// <copyright file="WatchlistModel.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Data.Models.User;

namespace KapitelShelf.Data.Models.Watchlists;

/// <summary>
/// The watchlist model.
/// </summary>
public class WatchlistModel
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the series id.
    /// </summary>
    public Guid SeriesId { get; set; }

    /// <summary>
    /// Gets or sets the series.
    /// </summary>
    public virtual SeriesModel Series { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public virtual UserModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last time this series was checked.
    /// </summary>
    public DateTime LastChecked { get; set; }
}
