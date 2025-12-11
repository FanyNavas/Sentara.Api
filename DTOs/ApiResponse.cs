namespace Sentara.Api.DTOs
{
    public class ApiResponse
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }

        public static ApiResponse Success() => new() { Ok = true };
        public static ApiResponse Fail(string error) => new() { Ok = false, Error = error };
    }
}
