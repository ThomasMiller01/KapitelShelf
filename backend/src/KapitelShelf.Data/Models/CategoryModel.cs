namespace KapitelShelf.Data.Models;

public class CategoryModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<BookCategoryModel> Books { get; set; } = new List<BookCategoryModel>();
}
