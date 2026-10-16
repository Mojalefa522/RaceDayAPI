using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class ParticipantProfile
    {
        [Key]
        public int ProfileID { get; set; }
        [Required]
        public int UserID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(20)]
        public string Gender { get; set; }
        [MaxLength(100)]
        public string EmergencyContact { get; set; }
        public string MedicalNotes { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}