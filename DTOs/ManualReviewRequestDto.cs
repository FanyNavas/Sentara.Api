namespace Sentara.Api.DTOs
{
    public class ManualReviewRequestDto
    {
        public string Teacher { get; set; } = null!;
        public string ClaimedName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string Reason { get; set; } = null!;

        public string? PhotoDataUrl { get; set; } // uploaded file
        public string? LiveDataUrl { get; set; }  // snapshot
    }
}

