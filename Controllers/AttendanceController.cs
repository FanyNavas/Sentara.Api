using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sentara.Api.Domain;
using Sentara.Api.DTOs;
using Sentara.Api.Infrastructure;

namespace Sentara.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class AttendanceController : ControllerBase
    {
        private readonly SentaraDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            SentaraDbContext db,
            IEmailSender emailSender,
            IWebHostEnvironment env,
            ILogger<AttendanceController> logger)
        {
            _db = db;
            _emailSender = emailSender;
            _env = env;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitAttendanceRequest request)
        {
            try
            {
                // ---- Basic validation ----
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid payload."));

                if (string.IsNullOrWhiteSpace(request.Teacher) ||
                    string.IsNullOrWhiteSpace(request.Student) ||
                    string.IsNullOrWhiteSpace(request.ClassName) ||
                    string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest(ApiResponse.Fail("Missing required fields."));
                }

                // ---- Save snapshot (if provided) ----
                string? fileName = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.SnapshotDataUrl))
                    {
                        fileName = SaveDataUrlAsPng(request.SnapshotDataUrl, "attendance");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save snapshot image.");
                    // do not fail the whole request just because image save failed
                }

                // ---- Persist attendance record ----
                var record = new AttendanceRecord
                {
                    TeacherEmail = request.Teacher.Trim(),
                    StudentName = request.Student.Trim(),
                    ClassName = request.ClassName.Trim(),
                    Status = request.Status.Trim(),
                    SnapshotFileName = fileName
                };

                _db.AttendanceRecords.Add(record);
                await _db.SaveChangesAsync();

                // ---- Build email body ----
                var subject = $"Sentara Attendance • {record.ClassName}";

                var body = $@"
                    <h2>Sentara Attendance</h2>
                    <p><strong>Student:</strong> {record.StudentName}</p>
                    <p><strong>Class:</strong> {record.ClassName}</p>
                    <p><strong>Status:</strong> {record.Status}</p>
                    <p><strong>Recorded at (UTC):</strong> {record.CreatedAt:u}</p>
                    {(string.IsNullOrWhiteSpace(record.SnapshotFileName)
                        ? string.Empty
                        : "<p>The snapshot image is attached to this email.</p>")}
                 ";

                // ---- Build attachments list (if file exists) ----
                var attachments = new List<EmailAttachment>();

                if (!string.IsNullOrWhiteSpace(record.SnapshotFileName))
                {
                    var folder = Path.Combine(_env.ContentRootPath, "Snapshots");
                    var fullPath = Path.Combine(folder, record.SnapshotFileName);

                    if (System.IO.File.Exists(fullPath))
                    {
                        attachments.Add(new EmailAttachment
                        {
                            FilePath = fullPath,
                            ContentType = "image/png",
                            FileName = $"attendance_{record.Id}.png"
                        });
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Snapshot file not found at {Path} for record {RecordId}",
                            fullPath, record.Id);
                    }
                }

                // ---- Send email (with optional attachment) ----
                await _emailSender.SendAsync(record.TeacherEmail, subject, body, attachments);

                return Ok(ApiResponse.Success());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in /api/submit");
                return StatusCode(500, ApiResponse.Fail("Server error: " + ex.Message));
            }
        }


        // POST /api/manual-review
        [HttpPost("manual-review")]
        public async Task<IActionResult> ManualReview([FromBody] ManualReviewRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid payload."));

                if (string.IsNullOrWhiteSpace(request.Teacher) ||
                    string.IsNullOrWhiteSpace(request.ClaimedName) ||
                    string.IsNullOrWhiteSpace(request.ClassName) ||
                    string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(ApiResponse.Fail("Missing required fields."));
                }

                string? uploadedFile = SaveDataUrlAsPng(request.PhotoDataUrl, "manual-upload");
                string? liveFile = SaveDataUrlAsPng(request.LiveDataUrl, "manual-live");

                var entity = new ManualReviewRequest
                {
                    TeacherEmail = request.Teacher,
                    ClaimedName = request.ClaimedName,
                    ClassName = request.ClassName,
                    Reason = request.Reason,
                    UploadedPhotoFileName = uploadedFile,
                    LiveSnapshotFileName = liveFile
                };

                _db.ManualReviewRequests.Add(entity);
                await _db.SaveChangesAsync();

                var subject = $"Sentara Manual Review • {entity.ClassName}";
                var body = $@"
                    <h2>Sentara Manual Review Request</h2>
                    <p><strong>Claimed student:</strong> {entity.ClaimedName}</p>
                    <p><strong>Class:</strong> {entity.ClassName}</p>
                    <p><strong>Reason:</strong> {entity.Reason}</p>
                    <p><strong>Requested at (Time):</strong> {entity.CreatedAt:u}</p>
                    <p>Images have been stored in the backend for your review.</p>";

                await _emailSender.SendAsync(entity.TeacherEmail, subject, body);

                return Ok(ApiResponse.Success());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in /api/manual-review");
                return StatusCode(500, ApiResponse.Fail("Server error: " + ex.Message));
            }
        }
        public async Task<ActionResult<ApiResponse>> ManualReviewRequest([FromBody] ManualReviewRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Fail("Invalid payload."));

            if (string.IsNullOrWhiteSpace(request.Teacher) ||
                string.IsNullOrWhiteSpace(request.ClaimedName) ||
                string.IsNullOrWhiteSpace(request.ClassName) ||
                string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(ApiResponse.Fail("Missing required fields."));
            }

            string? uploadedFile = null;
            string? liveFile = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(request.PhotoDataUrl))
                    uploadedFile = SaveDataUrlAsPng(request.PhotoDataUrl, "manual-upload");

                if (!string.IsNullOrWhiteSpace(request.LiveDataUrl))
                    liveFile = SaveDataUrlAsPng(request.LiveDataUrl, "manual-live");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save manual-review images.");
            }

            var entity = new ManualReviewRequest
            {
                TeacherEmail = request.Teacher,
                ClaimedName = request.ClaimedName,
                ClassName = request.ClassName,
                Reason = request.Reason,
                UploadedPhotoFileName = uploadedFile,
                LiveSnapshotFileName = liveFile
            };

            _db.ManualReviewRequests.Add(entity);
            await _db.SaveChangesAsync();

            var subject = $"Sentara Manual Review • {entity.ClassName}";
            var body = $@"
                <h2>Sentara Manual Review Request</h2>
                <p><strong>Claimed student:</strong> {entity.ClaimedName}</p>
                <p><strong>Class:</strong> {entity.ClassName}</p>
                <p><strong>Reason:</strong> {entity.Reason}</p>
                <p><strong>Requested at ():</strong> {entity.CreatedAt:u}</p>
                <p>Images have been stored in the backend for your review.</p>";

            try
            {
                await _emailSender.SendAsync(entity.TeacherEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send manual-review email.");
                return StatusCode(500, ApiResponse.Fail("Email send failed. Check SMTP settings."));
            }

            return Ok(ApiResponse.Success());
        }

        // Helper: saves "data:image/png;base64,..." as a .png file and returns file name
        private string SaveDataUrlAsPng(string dataUrl, string prefix)
        {
            if (string.IsNullOrWhiteSpace(dataUrl))
                return null;

            try
            {
                // data:image/png;base64,xxxx
                var parts = dataUrl.Split(',', 2);
                var base64 = parts.Length == 2 ? parts[1] : dataUrl;

                byte[] bytes = Convert.FromBase64String(base64);

                var folder = Path.Combine(_env.ContentRootPath, "Snapshots");
                Directory.CreateDirectory(folder);

                var fileName = $"{prefix}_{Guid.NewGuid():N}.png";
                var fullPath = Path.Combine(folder, fileName);

                System.IO.File.WriteAllBytes(fullPath, bytes);
                return fileName;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Invalid image data URL received; skipping image save.");
                return null; // don’t fail the whole request;
            }
        }
    }
}

