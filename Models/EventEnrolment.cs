using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class EventEnrolment
    {
        [Key]
        public int EnrolmentID { get; set; }
        [Required]
        public int CategoryID { get; set; }
        [Required]
        public int ParticipantID { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        [MaxLength(20)]
        public string BibNumber { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
        [ForeignKey("ParticipantID")]
        public User Participant { get; set; }
    }
}