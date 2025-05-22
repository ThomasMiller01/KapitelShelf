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
