using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        public int RoleID { get; set; }
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [ForeignKey("RoleID")]
        public Role Role { get; set; }
    }
}