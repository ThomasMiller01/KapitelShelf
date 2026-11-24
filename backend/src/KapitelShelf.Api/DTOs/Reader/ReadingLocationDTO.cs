// <copyright file="ReadingLocationDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KapitelShelf.Api.DTOs.Reader;

/// <summary>
/// The reading location dto.
/// </summary>
public class ReadingLocationDTO : IValidatableObject
{
    /// <summary>
    /// Gets or sets the current section.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CurrentSection { get; set; }

    /// <summary>
    /// Gets or sets the current page.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the versioned content anchor in the form 1.sha256.source.t-or-i.offset.
    /// </summary>
    [StringLength(96)]
    public string? ContentAnchor { get; set; }

    /// <summary>
    /// Gets or sets the revision this save was based on, or null for the first bookmark.
    /// </summary>
    public Guid? ExpectedRevision { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for a guarded save, reused on retries.
    /// </summary>
    public Guid? WriteId { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.WriteId == Guid.Empty || (this.WriteId is null && (this.ExpectedRevision is not null || this.ContentAnchor is not null)))
        {
            yield return new ValidationResult("A nonempty write id is required for revisioned saves.", [nameof(this.WriteId)]);
        }

        if (this.ExpectedRevision == Guid.Empty)
        {
            yield return new ValidationResult("Use null for an unsaved bookmark revision.", [nameof(this.ExpectedRevision)]);
        }

        if (this.ContentAnchor is { } value)
        {
            var parts = value.Split('.');
            if (parts.Length != 5 || parts[0] != "1" || parts[1].Length != 64
                || !parts[1].All(c => char.IsAsciiDigit(c) || c is >= 'a' and <= 'f')
                || (parts[3] != "t" && parts[3] != "i")
                || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out _)
                || !int.TryParse(parts[4], NumberStyles.None, CultureInfo.InvariantCulture, out _))
            {
                yield return new ValidationResult("The content anchor is invalid.", [nameof(this.ContentAnchor)]);
            }
        }
    }
}
