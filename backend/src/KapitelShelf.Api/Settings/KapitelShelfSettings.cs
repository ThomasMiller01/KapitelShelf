// <copyright file="KapitelShelfSettings.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Settings
{
    /// <summary>
    /// The KapitelShelf settings.
    /// </summary>
    public class KapitelShelfSettings
    {
        /// <summary>
        /// Gets or sets the kapitelshelf api domain.
        /// </summary>
        public string Domain { get; set; } = null!;

        /// <summary>
        /// Gets or sets the kapitelshelf data directory.
        /// </summary>
        public string DataDir { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database settings.
        /// </summary>
        public DatabaseSettings Database { get; set; } = null!;
    }
}
