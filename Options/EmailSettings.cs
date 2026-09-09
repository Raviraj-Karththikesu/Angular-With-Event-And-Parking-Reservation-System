namespace Event_and_parking_reservation_system.Options
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string FromName { get; set; }
            = "Event Parking Reservation System";

        public string FromAddress { get; set; }
            = string.Empty;

        public string FrontendBaseUrl { get; set; }
            = "http://localhost:4200";

        public string SmtpHost { get; set; }
            = "smtp.gmail.com";

        public int SmtpPort { get; set; }
            = 587;

        public string Username { get; set; }
            = string.Empty;

        public string Password { get; set; }
            = string.Empty;

        public int VerificationTokenExpiryHours { get; set; }
            = 24;

        public int PasswordResetTokenExpiryMinutes { get; set; }
            = 60;
    }
}