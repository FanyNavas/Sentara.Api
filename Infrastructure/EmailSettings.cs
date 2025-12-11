namespace Sentara.Api.Infrastructure
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string From { get; set; } = null!;
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
