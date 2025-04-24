// <copyright file="PagedResult.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs
{
    /// <summary>
    /// The paginated result.
    /// </summary>
    /// <typeparam name="T">The pagination items type.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IList<T> Items { get; set; } = [];

        /// <summary>
        /// Gets or sets the total number of items.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
