// <copyright file="CategoriesLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.Extensions;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="CategoriesLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
public class CategoriesLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : ICategoriesLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Mapper mapper = mapper;
#pragma warning restore IDE0052 // Remove unread private members

    /// <inheritdoc/>
    public async Task<PagedResult<CategoryDTO>> GetCategoriesAsync(int page, int pageSize, CategorySortByDTO sortBy, SortDirectionDTO sortDir, string? filter)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var query = context.Categories
            .AsNoTracking()
            .Include(x => x.Books)

            // apply filter if it is set
            .FilterByCategoryQuery(filter)
            .SortByCategoryQuery(filter);

        // apply sorting, if no filter is specified
        // or if a specific sorting is requested
        if (filter is null || sortBy != CategorySortByDTO.Default)
        {
            query = query.ApplySorting(sortBy, sortDir);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)

            .Select(x => this.mapper.CategoryModelToCategoryDto(x))
            .ToListAsync();

        var totalCount = await query.CountAsync();

        return new PagedResult<CategoryDTO>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task DeleteCategoriesAsync(List<Guid> categoryIdsToDelete)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        await context.Categories
            .Where(x => categoryIdsToDelete.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<List<string>> AutocompleteAsync(string? partialCategoryName)
    {
        if (string.IsNullOrWhiteSpace(partialCategoryName))
        {
            return [];
        }

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Categories
            .AsNoTracking()
            .FilterByCategoryQuery(partialCategoryName)
            .SortByCategoryQuery(partialCategoryName)
            .Take(5)
            .Select(x => x.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<CategoryDTO?> UpdateCategoryAsync(Guid categoryId, CategoryDTO categoryDto)
    {
        if (categoryDto is null)
        {
            return null;
        }

        // check for duplicate categories
        var duplicates = await this.GetDuplicatesAsync(categoryDto.Name);
        if (duplicates.Any())
        {
            throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var category = await context.Categories
            .FirstOrDefaultAsync(b => b.Id == categoryId);

        if (category is null)
        {
            return null;
        }

        // patch category root scalars
        context.Entry(category).CurrentValues.SetValues(new
        {
            categoryDto.Name,
            UpdatedAt = DateTime.UtcNow,
        });

        // commit
        await context.SaveChangesAsync();

        return this.mapper.CategoryModelToCategoryDto(category);
    }

    /// <inheritdoc/>
    public async Task MergeCategories(Guid targetCategoryId, List<Guid> sourceCategoryIds)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var targetCategory = await context.Categories.FirstOrDefaultAsync(x => x.Id == targetCategoryId);
        if (targetCategory is null)
        {
            throw new ArgumentException("Unknown target category id.");
        }

        var sourceCategories = await context.BookCategories
            .Include(x => x.Book)
            .Where(x => sourceCategoryIds.Contains(x.CategoryId))
            .ToListAsync();

        if (sourceCategories.Count == 0)
        {
            throw new ArgumentException("Unknown source category ids.");
        }

        // update all categories
        foreach (var category in sourceCategories)
        {
            context.BookCategories.Add(new BookCategoryModel
            {
                BookId = category.BookId,
                CategoryId = targetCategoryId,
            });

            category.Book.UpdatedAt = DateTime.UtcNow;
        }

        targetCategory.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // delete source categories
        await this.DeleteCategoriesAsync(sourceCategories.Select(x => x.CategoryId).ToList());
    }

    internal async Task<IList<CategoryModel>> GetDuplicatesAsync(string name)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Categories
            .AsNoTracking()
            .Where(x => x.Name == name)
            .ToListAsync();
    }
}
