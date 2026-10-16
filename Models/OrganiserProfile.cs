using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class OrganiserProfile
    {
        [Key]
        public int ProfileID { get; set; }
        [Required]
        public int UserID { get; set; }
        [MaxLength(100)]
        public string CompanyName { get; set; }
        [MaxLength(255)]
        public string Website { get; set; }
        [MaxLength(20)]
        public string ContactPhone { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}