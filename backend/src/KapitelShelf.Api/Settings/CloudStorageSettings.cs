// <copyright file="CloudStorageSettings.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Settings
{
    /// <summary>
    /// The database settings.
    /// </summary>
    public class CloudStorageSettings
    {
        /// <summary>
        /// Gets or sets the cloud storage rclone executable.
        /// </summary>
        public string RClone { get; set; } = null!;
    }
}
