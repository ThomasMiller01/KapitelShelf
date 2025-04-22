namespace KapitelShelf.Data.Models;

public class BookTagModel
{
    public Guid BookId { get; set; }
    public BookModel Book { get; set; } = null!;

    public Guid TagId { get; set; }
    public TagModel Tag { get; set; } = null!;
}
