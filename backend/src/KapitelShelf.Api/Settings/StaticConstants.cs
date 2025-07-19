// <copyright file="StaticConstants.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Settings
{
    /// <summary>
    /// The static constants.
    /// </summary>
    public class StaticConstants
    {
        /// <summary>
        /// The name of the database.
        /// </summary>
        public static readonly string DatabaseName = "kapitelshelf";

        /// <summary>
        /// The key for the duplicate item exception.
        /// </summary>
        public static readonly string DuplicateExceptionKey = "DUPLICATE";

        /// <summary>
        /// The text for the exception, when amazon blocked us.
        /// </summary>
        public static readonly string MetadataScrapingBlockedKey = "METADATA_SCRAPING_BLOCKED";

        /// <summary>
        /// The maximum number of search suggestions to return.
        /// </summary>
        public static readonly int MaxSearchSuggestions = 12;

        /// <summary>
        /// The cloud storage configuration directory subpath.
        /// </summary>
        public static readonly string CloudStorageConfigurationSubPath = "cloudstorage";

        /// <summary>
        /// The cloud storage cloud data subpath.
        /// </summary>
        public static readonly string CloudStorageCloudDataSubPath = "clouddata";

        /// <summary>
        /// The cloud storage rclone filename.
        /// </summary>
        public static readonly string CloudStorageRCloneFileName = "rclone.conf";

        /// <summary>
        /// The name of the rclone config.
        /// </summary>
        public static readonly string CloudStorageRCloneConfigName = "kapitelshelf";

        /// <summary>
        /// The the cloud storage directory not found exception.
        /// </summary>
        public static readonly string CloudStorageDirectoryNotFoundExceptionKey = "CLOUDSTORAGE_DIRECTORY_NOT_FOUND";

        /// <summary>
        /// The cloud storage storage not found exception.
        /// </summary>
        public static readonly string CloudStorageStorageNotFoundExceptionKey = "CLOUDSTORAGE_STORAGE_NOT_FOUND";
    }
}
