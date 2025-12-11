using Microsoft.EntityFrameworkCore;
using Sentara.Api.Domain;

namespace Sentara.Api.Infrastructure
{
    public class SentaraDbContext : DbContext
    {
        public SentaraDbContext(DbContextOptions<SentaraDbContext> options)
            : base(options)
        {
        }

        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<ManualReviewRequest> ManualReviewRequests => Set<ManualReviewRequest>();
    }
}
