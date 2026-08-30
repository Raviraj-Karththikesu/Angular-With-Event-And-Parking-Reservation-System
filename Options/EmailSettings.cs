namespace Event_and_parking_reservation_system.Options
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string FromName { get; set; }
            = "Event Parking Reservation System";

        public string FromAddress { get; set; }
            = "no-reply@eventparking.local";

        public string FrontendBaseUrl { get; set; }
            = "http://localhost:4200";

        public int VerificationTokenExpiryHours { get; set; }
            = 24;

        public int PasswordResetTokenExpiryMinutes { get; set; }
            = 60;
    }
}