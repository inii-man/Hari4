using System.ComponentModel.DataAnnotations;

namespace ProductWeb.Models;

/// <summary>DTO untuk form login — dikirim ke endpoint /api/auth/login.</summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Response dari endpoint login yang berisi JWT token.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
    public string Password { get; set; } = string.Empty;

    // Secara default user tidak akan dikasih opsi checkbox admin role, melainkan "User" di set otomatis. Tapi tidak ada salahnya disediakan jika dibutuhkan form masa depan
    public string Role { get; set; } = "User";
}
