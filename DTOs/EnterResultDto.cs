namespace RaceDayAPI.DTOs
{
    public class EnterResultDto
    {
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? Notes { get; set; }
    }
}