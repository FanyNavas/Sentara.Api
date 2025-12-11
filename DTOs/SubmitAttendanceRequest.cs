namespace Sentara.Api.DTOs
{
    public class SubmitAttendanceRequest
    {
        public string Teacher { get; set; } = null!;
        public string Student { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string Status { get; set; } = null!;           // "yes"
        public string? SnapshotDataUrl { get; set; }          // "data:image/png;base64,..."
    }
}

