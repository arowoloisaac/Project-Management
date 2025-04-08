namespace Task_Management_System.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }

        public TokenResponse(string Token)
        {
            this.Token = Token;
        }
    }
}
