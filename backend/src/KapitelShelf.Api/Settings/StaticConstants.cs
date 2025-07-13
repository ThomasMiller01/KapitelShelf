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
        /// The default OneDrive clientId of RClone.
        /// </summary>
        public static readonly string CloudStorageOneDriveRCloneClientId = "TODO";
    }
}
