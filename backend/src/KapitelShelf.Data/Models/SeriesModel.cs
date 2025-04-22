namespace KapitelShelf.Data.Models;

public class SeriesModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<BookModel> Books { get; set; } = new List<BookModel>();
}
