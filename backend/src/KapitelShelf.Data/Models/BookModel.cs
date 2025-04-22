namespace KapitelShelf.Data.Models
{
    public class BookModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public DateTime? ReleaseDate { get; set; }

        public int? PageNumber { get; set; }

        public Guid? SeriesId { get; set; }
        public SeriesModel? Series { get; set; }
        public int? SeriesNumber = null;

        public Guid AuthorId { get; set; }
        public AuthorModel? Author { get; set; }

        public ICollection<BookCategoryModel> Categories { get; set; } = new List<BookCategoryModel>();
        public ICollection<BookTagModel> Tags { get; set; } = new List<BookTagModel>();

        public FileInfoModel? Cover { get; set; }
        public LocationModel? Location { get; set; }
    }
}
