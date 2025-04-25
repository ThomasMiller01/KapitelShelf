// <copyright file="DatabaseSettings.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Settings
{
    /// <summary>
    /// The database settings.
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// Gets or sets the database host.
        /// </summary>
        public string Host { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database Username.
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database password.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
