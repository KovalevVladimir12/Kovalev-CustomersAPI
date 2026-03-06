namespace Kovalev.Models.Auth;

public class AuthResponse
{
    public string? Token { get; set; }      
    public string? Username { get; set; }  
    public string? Email { get; set; }      
    public DateTime Expiration { get; set; }
}