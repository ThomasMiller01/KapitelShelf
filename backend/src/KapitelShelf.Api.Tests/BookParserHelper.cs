// <copyright file="BookParserHelper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace KapitelShelf.Api.Tests;

/// <summary>
/// Helper class for book parser tests.
/// </summary>
public static class BookParserHelper
{
    /// <summary>
    /// The cover bytes of a 1x1 pixel PNG image.
    /// </summary>
    public static readonly byte[] CoverBytes =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
        0x54, 0x08, 0xD7, 0x63, 0xF8, 0x0F, 0x00, 0x01,
        0x01, 0x01, 0x00, 0x18, 0xDD, 0x8E, 0x9B, 0x00,
        0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82,
    ];

    /// <summary>
    /// Represents an in-memory implementation of <see cref="IFormFile"/> for testing.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="fileName">The file name.</param>
    public sealed class InMemoryFormFile(byte[] data, string fileName) : IFormFile, IDisposable
    {
        private readonly MemoryStream stream = new(data);

        /// <inheritdoc/>
        public string ContentType => "application/pdf";

        /// <inheritdoc/>
        public IHeaderDictionary Headers => null!;

        /// <inheritdoc/>
        public long Length { get; } = data.Length;

        /// <inheritdoc/>
        public string Name => "file";

        /// <inheritdoc/>
        public string FileName { get; } = fileName;

        /// <inheritdoc/>
        public string ContentDisposition => string.Empty;

        /// <inheritdoc/>
        public void CopyTo(Stream target) => stream.CopyTo(target);

        /// <inheritdoc/>
        public Stream OpenReadStream()
        {
            stream.Position = 0;
            return stream;
        }

        /// <inheritdoc/>
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => stream.CopyToAsync(target, cancellationToken);

        /// <inheritdoc/>
        public void Dispose() => this.stream.Dispose();
    }
}
