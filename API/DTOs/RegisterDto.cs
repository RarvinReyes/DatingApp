using System.ComponentModel.DataAnnotations;

namespace API.DTOs;
public class RegisterDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    [StringLength(15, MinimumLength = 5)]
    public string Password { get; set; }
}
