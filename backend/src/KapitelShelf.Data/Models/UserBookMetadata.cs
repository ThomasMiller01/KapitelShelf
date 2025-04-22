namespace KapitelShelf.Data.Models
{
    public class UserBookMetadata
    {
        public Guid Id { get; set; }

        public Guid BookId { get; set; }
        public BookModel Book { get; set; } = null!;

        public int? Rating { get; set; }
        public bool? Favourite { get; set; }
        public DateTime? LastReadDate { get; set; }
        public string? Comment { get; set; }

        public Guid UserId { get; set; }
        public UserModel User { get; set; } = null!;
    }
}
