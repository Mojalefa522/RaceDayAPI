using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
    }
}