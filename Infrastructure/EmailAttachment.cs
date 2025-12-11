namespace Sentara.Api.Infrastructure
{
    public class EmailAttachment
    {
        public string FilePath { get; set; } = default!;
        public string ContentType { get; set; } = "application/octet-stream";
        public string? FileName { get; set; }
    }
}