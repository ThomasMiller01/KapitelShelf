// <copyright file="ReadingProgressConflictException.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Reader;

namespace KapitelShelf.Api.Logic;

internal sealed class ReadingProgressConflictException(ReadingBookDTO bookmark) : Exception("The reading bookmark has changed.")
{
    public ReadingBookDTO Bookmark { get; } = bookmark;
}
