// <copyright file="BookParserBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using KapitelShelf.Api.DTOs.BookParser;
using KapitelShelf.Api.Logic.Interfaces.BookParser;

namespace KapitelShelf.Api.Logic.BookParser;

/// <summary>
/// The base class for the book parsers, containing common functionality.
/// </summary>
public abstract partial class BookParserBase : IBookParser
{
    /// <inheritdoc/>
    public abstract IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <inheritdoc/>
    public abstract Task<BookParsingResult> Parse(IFormFile file);

    /// <inheritdoc/>
    public abstract Task<List<BookParsingResult>> ParseBulk(IFormFile file);

    /// <summary>
    /// Sanitize and remove html-tags and similar from the text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The sanitized text.</returns>
    protected string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var removedHtml = SanitizeHtmlRegex().Replace(text, string.Empty);

        return removedHtml;
    }

    /// <summary>
    /// Parse the authors first- and lastname from an author string.
    /// </summary>
    /// <param name="author">The author string.</param>
    /// <returns>The first and last name of the author.</returns>
    protected (string firstName, string lastName) ParseAuthor(string author)
    {
        if (string.IsNullOrEmpty(author))
        {
            return (string.Empty, string.Empty);
        }

        // if the author has the following format: "LastName, FirstName"
        // convert it to "FirstName LastName"
        if (author.Contains(','))
        {
            var authorParts = author
                .Split(",")
                .Select(x => x.Trim()); // remove trailing/leading white spaces

            // rebuild with white spaces, but reversed, so you get "FirstName LastName"
            author = string.Join(" ", authorParts.Reverse());
        }

        // split author into firstName and lastName
        var parts = author.Split(" ");
        string firstName;
        string lastName;
        if (parts.Length == 1)
        {
            // no last name
            firstName = parts.First();
            lastName = string.Empty;
        }
        else
        {
            // everything except last goes into firstName
            firstName = string.Join(" ", parts.Take(parts.Length - 1));
            lastName = parts.Last();
        }

        return (firstName, lastName);
    }

    /// <summary>
    /// Extracts a readable book title from a string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <returns>The parsed title.</returns>
    protected string ParseTitle(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        // Matches any character NOT a letter, digit, space, hyphen, apostrophe, comma, colon, or period.
        var cleaned = AllowedTitleCharactersRegex().Replace(str, " ");

        // Collapse multiple spaces into one
        var collapsedSpaces = MultipleSpacesRegex().Replace(cleaned, " ");

        // Trim leading/trailing spaces
        var trimmed = collapsedSpaces.Trim();

        return trimmed;
    }

    /// <summary>
    /// Parses the title from the file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The parsed title.</returns>
    protected string ParseTitleFromFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        var parts = fileName.Split('.');
        var withoutExtension = string.Join(".", parts.Take(parts.Length - 1));

        return this.ParseTitle(withoutExtension);
    }

    // Matches any character that is NOT a letter, digit, space, hyphen, apostrophe, comma, colon, or period
    // Use \p{L] for unicode letters (any alphabet, cyrillic, lating, greek, arabic, etc.)
    [GeneratedRegex(@"[^\p{L}0-9 ',:\.-]", RegexOptions.Compiled)]
    private static partial Regex AllowedTitleCharactersRegex();

    // Matches one or more consecutive whitespace characters (for collapsing spaces)
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleSpacesRegex();

    // Matches any '<...>' (non-greedy)
    [GeneratedRegex("<.*?>", RegexOptions.Compiled)]
    private static partial Regex SanitizeHtmlRegex();
}
