﻿// <copyright file="KapitelShelfSettings.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Migrations.Settings
{
    /// <summary>
    /// The KapitelShelf settings.
    /// </summary>
    public class KapitelShelfSettings
    {
        /// <summary>
        /// Gets or sets the database settings.
        /// </summary>
        public DatabaseSettings Database { get; set; } = null!;
    }
}
