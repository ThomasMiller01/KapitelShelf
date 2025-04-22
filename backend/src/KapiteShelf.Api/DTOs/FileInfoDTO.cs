namespace KapitelShelf.Api.DTOs;

public class FileInfoDTO
{
    public Guid Id { get; set; }

    public string FilePath { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = null!;

    public string Sha256 { get; set; } = null!;
}
