namespace KapitelShelf.Data.Models
{
    public class TagModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<BookTagModel> Books { get; set; } = null!;
    }
}
