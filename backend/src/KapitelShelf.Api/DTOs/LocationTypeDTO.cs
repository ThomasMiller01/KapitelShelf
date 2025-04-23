// <copyright file="LocationTypeDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs
{
    /// <summary>
    /// The location type enum.
    /// </summary>
    public enum LocationTypeDTO
    {
        /// <summary>
        /// A physical book.
        /// </summary>
        Physical,

        /// <summary>
        /// Hosted in KapiteShelf.
        /// </summary>
        KapitelShelf,

        /// <summary>
        /// A book from kindle.
        /// </summary>
        Kindle,

        /// <summary>
        /// A book from skoobe.
        /// </summary>
        Skoobe,

        /// <summary>
        /// A book from onleihe.
        /// </summary>
        Onleihe,

        /// <summary>
        /// A book in a library.
        /// </summary>
        Library,
    }
}
