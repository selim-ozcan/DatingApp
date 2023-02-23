using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [MaxLength(8)]
        [MinLength(4)]
        [Required]
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}