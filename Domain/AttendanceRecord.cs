namespace Sentara.Api.Domain
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public string TeacherEmail { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public string Status { get; set; } = null!; // "yes" etc.

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Where we store the image file name (PNG) saved on disk
        public string? SnapshotFileName { get; set; }
    }
}
