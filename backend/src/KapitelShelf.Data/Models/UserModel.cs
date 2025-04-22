namespace KapitelShelf.Data.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;

    public ICollection<UserBookMetadata> Books { get; set; } = new List<UserBookMetadata>();
}
