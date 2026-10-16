namespace RaceDayAPI.DTOs
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Distance { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int MaxCapacity { get; set; }
    }
}