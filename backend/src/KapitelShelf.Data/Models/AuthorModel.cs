namespace KapitelShelf.Data.Models;

public class AuthorModel
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public ICollection<BookModel> Books { get; set; } = new List<BookModel>(); 
}
