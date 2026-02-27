namespace Event_Management_System.API.Helpers
{
    public class EmailSettings
    {
        public string DefaultFromEmail { get; set; } = string.Empty;
        public string DefaultFromName { get; set; } = string.Empty;
        public SmtpSettings SMTPSetting { get; set; } = new();
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
    }
}
