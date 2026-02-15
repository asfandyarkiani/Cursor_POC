namespace SystemLayer.Application.DTOs;

public class LoginRequestDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Database { get; set; }
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? SessionToken { get; set; }
    public string? UserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Errors { get; set; }
}

public class LogoutRequestDto
{
    public string? SessionToken { get; set; }
}

public class LogoutResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}