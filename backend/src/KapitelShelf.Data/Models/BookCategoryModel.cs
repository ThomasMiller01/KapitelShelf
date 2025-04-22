namespace KapitelShelf.Data.Models
{
    public class BookCategoryModel
    {
        public Guid BookId { get; set; }
        public BookModel Book { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public CategoryModel Category { get; set; } = null!;
    }
}
