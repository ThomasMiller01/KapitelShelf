// <copyright file="BookParserBase.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;
using KapitelShelf.Api.DTOs.BookParser;

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

    // "<.*?>" matches any '<...>' (non-greedy)
    [GeneratedRegex("<.*?>")]
    private static partial Regex SanitizeHtmlRegex();
}
