using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string KnownAs { get; set; }

        [Required]
        public DateOnly? DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        [MaxLength(8)]
        [MinLength(4)]
        [Required]
        public string Password { get; set; }

        public string PasswordConfirm { get; set; }
    }
}