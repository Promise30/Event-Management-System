namespace Event_Management_System.API.Helpers
{
    public  class JwtSettings
    {
        public string ValidIssuer { get; set; } = null!;
        public string ValidAudience { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
            public int Expires { get; set; }

    }
}
