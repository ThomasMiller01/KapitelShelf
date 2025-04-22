namespace KapitelShelf.Api.DTOs;

public class AuthorDTO
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
