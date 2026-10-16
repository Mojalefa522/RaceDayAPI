using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }
        [Required]
        public int OrganiserID { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        [MaxLength(100)]
        public string Location { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("OrganiserID")]
        public User Organiser { get; set; }
    }
}