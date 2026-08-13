namespace API.DTOs
{
    public class UserDto
    {
        public string DispayName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Image { get; set; }
    }
}