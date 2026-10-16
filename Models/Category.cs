using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }
        [Required]
        public int EventID { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Distance { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        [Required]
        public int MaxCapacity { get; set; }
        public int CurrentEnrolments { get; set; } = 0;

        [ForeignKey("EventID")]
        public Event Event { get; set; }
    }
}