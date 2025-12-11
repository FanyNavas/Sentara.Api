namespace Sentara.Api.Domain
{
    public class ManualReviewRequest
    {
        public int Id { get; set; }

        public string TeacherEmail { get; set; } = null!;
        public string ClaimedName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // You can store paths to the saved images (PNG)
        public string? UploadedPhotoFileName { get; set; }
        public string? LiveSnapshotFileName { get; set; }
    }
}

