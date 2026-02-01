// <copyright file="AiPrompts.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.Tag;

namespace KapitelShelf.Api.Resources;

/// <summary>
/// The ai prompts.
/// </summary>
public class AiPrompts
{
    /// <summary>
    /// Generate categories and tags from a book, system prompt.
    /// </summary>
    public static readonly string GenerateCategoriesAndTagsFromBook_System = """
        You are a library assistant for a personal book manager.
        Generate categories and tags for a book based on the provided information.
        Return ONLY valid JSON with exactly these keys: categories, tags.
        Both values must be arrays of strings.
        Do not include any extra text, markdown, or explanations.
    """;

    /// <summary>
    /// Generate categories and tags from a book, user prompt.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="series">The series.</param>
    /// <param name="author">The author.</param>
    /// <param name="bookCategories">The book categories.</param>
    /// <param name="bookTags">The book tags.</param>
    /// <param name="libraryCategories">The library categories.</param>
    /// <param name="libraryTags">The library tags.</param>
    /// <returns>The user prompt.</returns>
    public static string GenerateCategoriesAndTagsFromBook_User(
        string title,
        string description,
        string series,
        string author,
        IList<CategoryDTO> bookCategories,
        IList<TagDTO> bookTags,
        IList<CategoryDTO> libraryCategories,
        IList<TagDTO> libraryTags) => $"""
        Book information:
        Title: {title}
        Description: {description}
        Series: {series}
        Author: {author}

        Existing metadata of this book:
        Categories: {string.Join(", ", bookCategories.Select(x => x.Name).ToList())}
        Tags: {string.Join(", ", bookTags.Select(x => x.Name).ToList())}

        Existing categories in the library (prefer using these when appropriate):
        {string.Join(", ", libraryCategories.Select(x => x.Name).ToList())}

        Existing tags in the library (prefer using these when appropriate):
        {string.Join(", ", libraryTags.Select(x => x.Name).ToList())}

        Rules:
        - categories: 0-3 items, genres or high-level topics
        - tags: 0-5 items, subgenres, tropes, themes, settings, or tone

        - be conservative and accuracy-focused:
            - only assign a specific genre (e.g. "Science Fiction", "Fantasy") if the title/description supports it
            - do not guess a genre when the information is unclear
            - if unsure, return fewer categories rather than an incorrect one

        - existing library categories/tags are suggestions only:
            - reuse them if they match the book
            - do not choose a category just because it exists in the library or is common

        - avoid generic labels like "Book", "Novel", "Story"
        - use English-style title casing for all categories and tags
        - do not invent author names or series
        - do not include duplicates
    """;
}
