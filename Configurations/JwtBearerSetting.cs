namespace Task_Management_System.Configurations
{
    public class JwtBearerSetting
    {
        public string SecretKey { get; set; } = string.Empty;

        public string Audience { get; set; }

        public string Issuer { get; set; }

        public long ExpiryTimeInSeconds { get; set; }
    }
}
