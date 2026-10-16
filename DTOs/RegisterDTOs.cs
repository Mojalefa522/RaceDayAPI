namespace RaceDayAPI.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string? CompanyName { get; set; }
        public string? Website { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }
    }
}