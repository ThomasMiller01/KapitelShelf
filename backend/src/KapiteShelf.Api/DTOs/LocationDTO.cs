// <copyright file="LocationDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs
{
    /// <summary>
    /// The location dto.
    /// </summary>
    public class LocationDTO
    {
        /// <summary>
        /// Gets or sets the locaiton id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public LocationTypeDTO Type { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the fileinfo.
        /// </summary>
        public FileInfoDTO? FileInfo { get; set; }
    }
}
