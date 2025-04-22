namespace KapitelShelf.Data.Models
{
    public class FileInfoModel
    {
        public Guid Id { get; set; }

        public string FilePath { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string MimeType { get; set; } = null!;

        public string Sha256 { get; set; } = null!;
    }
}
