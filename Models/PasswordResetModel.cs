namespace SJOB_EXE201.Models
{
    public class PasswordResetModel
    {
        public string Email { get; set; }
        public string OTP { get; set; }
        public DateTime Expiration { get; set; }
    }
}
