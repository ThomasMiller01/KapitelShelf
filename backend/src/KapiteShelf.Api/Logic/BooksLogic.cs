using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace KapitelShelf.Api.Logic;

public class BooksLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory;
    private readonly IMapper mapper;

    public BooksLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper)
    {
        this.dbContextFactory = dbContextFactory;
        this.mapper = mapper;
    }

    public async Task<IList<BookDTO>> GetBooksAsync()
    {
        using (var context = await this.dbContextFactory.CreateDbContextAsync())
        {
            return await context.Books
                .AsNoTracking()
                .Select(x => this.mapper.Map<BookDTO>(x))
                .ToListAsync();
        }
    }

    public async Task<BookDTO?> GetBookByIdAsync(Guid bookId)
    {
        using (var context = await this.dbContextFactory.CreateDbContextAsync())
        {
            return await context.Books
                .AsNoTracking()
                .Where(x => x.Id == bookId)
                .Select(x => this.mapper.Map<BookDTO>(x))
                .FirstOrDefaultAsync();
        }
    }

    public async Task<BookDTO> CreateBook(BookDTO bookDto)
    {
        using (var context = await this.dbContextFactory.CreateDbContextAsync())
        {
            var duplicates = await this.GetDuplicates(bookDto);
            if (duplicates.Any())
            {
                throw new InvalidOperationException(StaticConstants.DuplicateExceptionKey);
            }

            var book = this.mapper.Map<BookModel>(bookDto);
            book.Id = Guid.NewGuid();

            context.Books.Add(book);
            await context.SaveChangesAsync();

            return this.mapper.Map<BookDTO>(book);
        }
    }

    private async Task<IList<BookModel>> GetDuplicates(BookDTO bookDto)
    {
        using (var context = await this.dbContextFactory.CreateDbContextAsync())
        {
            var bookDtoSHA = bookDto.Location?.FileInfo?.Sha256;

            return await context.Books
            .AsNoTracking()
                .Where(x => x.Title == bookDto.Title)
                .ToListAsync();
        }
    }
}
