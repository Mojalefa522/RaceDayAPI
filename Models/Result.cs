using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }
        [Required]
        public int EnrolmentID { get; set; }
        [Column(TypeName = "time(3)")]
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "entered";
        [MaxLength(500)]
        public string Notes { get; set; }

        [ForeignKey("EnrolmentID")]
        public EventEnrolment Enrolment { get; set; }
    }
}