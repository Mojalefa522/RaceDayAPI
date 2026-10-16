namespace RaceDayAPI.DTOs
{
    public class UpdateParticipantProfileDto
    {
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }
    }
}